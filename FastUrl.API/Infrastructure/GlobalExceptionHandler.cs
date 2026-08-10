using System;
using System.Threading;
using System.Threading.Tasks;
using FastUrl.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FastUrl.API.Infrastructure;

/// <summary>
/// Bộ định dạng lỗi trung tâm .NET 8 (Centralized IExceptionHandler):
/// Bắt tất cả Exception toàn cục và đóng gói thành bản tin JSON RFC 7807 Problem Details đồng bộ.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode = exception switch
        {
            SecurityViolationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        string title = exception switch
        {
            SecurityViolationException => "Security Violation",
            ArgumentException => "Bad Request",
            _ => "Internal Server Error"
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Trả về true báo hiệu cho .NET 8 biết: Exception đã được xử lý xong!
        return true;
    }
}
