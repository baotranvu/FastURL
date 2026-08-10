using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FastUrl.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace FastUrl.API.Middleware;

/// <summary>
/// Middleware bảo vệ an ninh cửa ngõ API (Layer 2 Security Guard):
/// 1. Early Return kiểm tra đúng route POST /api/v1/urls
/// 2. Bật Buffering đọc Request Body an toàn (Stream Rewind)
/// 3. Scheme Whitelist (chỉ cho phép http:// và https://)
/// 4. Chống vòng lặp tự thân Self-Loop (chặn rút gọn domain hệ thống)
/// </summary>
public class UrlSafetyMiddleware
{
    private readonly RequestDelegate _next;

    // Danh sách các Domain hệ thống bị cấm rút gọn để chống vòng lặp vô tận (Self-Loop Prevention)
    private static readonly string[] BannedSelfDomains = new[]
    {
        "localhost",
        "127.0.0.1",
        "fasturl.api"
    };

    public UrlSafetyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // BƯỚC 1: Rẽ nhánh sớm (Early Return) - Chỉ can thiệp đúng POST /api/v1/urls
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !context.Request.Path.Equals("/api/v1/urls", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // BƯỚC 2: Bật Buffering cho phép đọc Stream và Rewind lại vị trí ban đầu (Tránh làm rỗng Body của Controller)
        context.Request.EnableBuffering();

        string? urlString = await ExtractUrlFromBodyAsync(context.Request.Body);

        // Reset vị trí Stream về 0 cho các Middleware tiếp theo & Controller đọc DTO
        context.Request.Body.Position = 0;

        if (!string.IsNullOrWhiteSpace(urlString))
        {
            // BƯỚC 3: Parse URL an toàn ở cấp độ C# CLR Native
            if (!Uri.TryCreate(urlString, UriKind.Absolute, out var uri))
            {
                throw new SecurityViolationException("Invalid URL format.");
            }

            // BƯỚC 4: BẢO VỆ 1 - Scheme Whitelist (Chỉ chấp nhận http:// và https://)
            if (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityViolationException("Security violation: Only HTTP and HTTPS URLs are allowed.");
            }

            // BƯỚC 5: BẢO VỆ 2 - Self-Loop Prevention (Chặn rút gọn chính link hệ thống)
            foreach (var bannedDomain in BannedSelfDomains)
            {
                if (uri.Host.Equals(bannedDomain, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityViolationException("Self-referential URLs from this domain cannot be shortened.");
                }
            }
        }

        // HỢP LỆ -> Cho phép Request đi tiếp vào Controller
        await _next(context);
    }

    private static async Task<string?> ExtractUrlFromBodyAsync(Stream bodyStream)
    {
        using var reader = new StreamReader(bodyStream, Encoding.UTF8, leaveOpen: true);
        string bodyText = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(bodyText))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(bodyText);
            if (doc.RootElement.TryGetProperty("url", out var urlElement))
            {
                return urlElement.GetString();
            }
        }
        catch (JsonException)
        {
            // Định dạng JSON không hợp lệ -> Để cho Model Binder / Validator xử lý tiếp
        }

        return null;
    }
}
