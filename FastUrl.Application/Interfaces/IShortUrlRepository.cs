using System.Threading;
using System.Threading.Tasks;
using FastUrl.Domain.Entities;

namespace FastUrl.Application.Interfaces;

/// <summary>
/// Repository Interface định nghĩa hợp đồng lưu trữ cho ShortUrl (Clean Architecture)
/// </summary>
public interface IShortUrlRepository
{
    Task AddAsync(ShortUrl shortUrl, CancellationToken cancellationToken = default);
    Task<ShortUrl?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default);
    Task IncrementAccessCountAsync(string shortCode, CancellationToken cancellationToken = default);
    Task<(System.Collections.Generic.IReadOnlyList<ShortUrl> Items, long? NextCursor, bool HasMore)> GetPagedAsync(long? cursor, int limit, CancellationToken cancellationToken = default);
}

