using System.Net;
using StackExchange.Redis;

namespace FastUrl.API.Middleware;

public class SlidingWindowRateLimiterMiddleware(
    RequestDelegate next,
    IConnectionMultiplexer? redis,
    ILogger<SlidingWindowRateLimiterMiddleware> logger)
{
    private const int DefaultLimit = 100; // 100 requests per window
    private const int WindowSeconds = 60; // 60 seconds rolling window

    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation("RateLimiter invoked for Path: {Path}. Redis null: {IsNull}, IsConnected: {IsConnected}", context.Request.Path, redis == null, redis?.IsConnected);

        // 1. Nếu Redis connection không sẵn sàng, log warning và fallback qua an toàn (Fail-Open Pattern)
        if (redis == null || !redis.IsConnected)
        {
            logger.LogWarning("Redis connection unavailable. Rate limiting skipped (Fail-Open).");
            await next(context);
            return;
        }


        try
        {
            // 2. Xác định Client Identifier (IP Address hoặc API Key Header)
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_client";
            string key = $"rate:{clientIp}";

            var db = redis.GetDatabase();
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long windowStart = now - (WindowSeconds * 1000);
            string member = $"{now}:{Guid.NewGuid():N}";

            // 1. Trượt cửa sổ bên trái (xóa phần tử hết hạn)
            await db.SortedSetRemoveRangeByScoreAsync(key, 0, windowStart);

            // 2. Thêm request hiện tại vào cửa sổ
            await db.SortedSetAddAsync(key, member, now);

            // 3. Đếm số phần tử trong cửa sổ 60s
            long currentRequestCount = await db.SortedSetLengthAsync(key);

            // 4. Thiết lập TTL 61s để dọn rác RAM
            _ = db.KeyExpireAsync(key, TimeSpan.FromSeconds(WindowSeconds + 1));

            // 5. Nếu vượt quá hạn ngạch -> Trả về HTTP 429 Too Many Requests
            if (currentRequestCount > DefaultLimit)
            {
                logger.LogWarning("Rate limit exceeded for Client IP {ClientIp}. Count: {Count}/{Limit}", clientIp, currentRequestCount, DefaultLimit);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\": 429, \"title\": \"Too Many Requests\", \"detail\": \"API rate limit exceeded. Maximum 100 requests per 60 seconds allowed.\"}");
                return;
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing Redis Sliding Window Rate Limiter. Falling back to allow request.");
        }

        // 5. Nếu nằm trong hạn ngạch -> Cho phép đi tiếp sang Middleware/Controller tiếp theo
        await next(context);
    }
}
