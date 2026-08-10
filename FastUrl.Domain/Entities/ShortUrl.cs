using System;

namespace FastUrl.Domain.Entities;

/// <summary>
/// Domain Entity đại diện cho liên kết rút gọn trong hệ thống
/// </summary>
public class ShortUrl
{
    public long Id { get; private set; }
    public string ShortCode { get; private set; } = string.Empty;
    public string OriginalUrl { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public long AccessCount { get; private set; }

    // Constructor dùng cho EF Core
    private ShortUrl() { }

    public ShortUrl(long id, string shortCode, string originalUrl)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "ID must be a positive integer.");
        }

        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code cannot be empty.", nameof(shortCode));
        }

        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            throw new ArgumentException("Original URL cannot be empty.", nameof(originalUrl));
        }

        Id = id;
        ShortCode = shortCode;
        OriginalUrl = originalUrl;
        CreatedAtUtc = DateTime.UtcNow;
        AccessCount = 0;
    }

    public void IncrementAccessCount()
    {
        AccessCount++;
    }
}
