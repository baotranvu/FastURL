using System;

namespace FastUrl.Domain.Events;

/// <summary>
/// Domain Event phát ra khi một link ngắn được rút gọn thành công (Dùng cho Outbox Pattern / Analytics)
/// </summary>
public record UrlShortenedEvent(
    long ShortUrlId,
    string ShortCode,
    string OriginalUrl,
    DateTime CreatedAtUtc
);
