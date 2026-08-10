using System;
using System.Threading;
using System.Threading.Tasks;
using FastUrl.Application.Interfaces;
using FastUrl.Domain.Entities;
using FastUrl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FastUrl.Infrastructure.Repositories;

/// <summary>
/// EF Core triển khai Repository Pattern cho IShortUrlRepository
/// </summary>
public class EfShortUrlRepository : IShortUrlRepository
{
    private readonly FastUrlDbContext _dbContext;

    public EfShortUrlRepository(FastUrlDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(ShortUrl shortUrl, CancellationToken cancellationToken = default)
    {
        await _dbContext.ShortUrls.AddAsync(shortUrl, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShortUrl?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        // Sử dụng B-Tree Unique Index trên ShortCode để tìm kiếm O(log N)
        return await _dbContext.ShortUrls
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ShortCode == shortCode, cancellationToken);
    }

    public async Task IncrementAccessCountAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        var shortUrl = await _dbContext.ShortUrls
            .FirstOrDefaultAsync(x => x.ShortCode == shortCode, cancellationToken);

        if (shortUrl != null)
        {
            shortUrl.IncrementAccessCount();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Phân trang dữ liệu lớn theo thuật toán con trỏ Cursor Seek O(log N) lợi dụng Primary Key B-Tree Index trên Id
    /// </summary>
    public async Task<(System.Collections.Generic.IReadOnlyList<ShortUrl> Items, long? NextCursor, bool HasMore)> GetPagedAsync(
        long? cursor, int limit, CancellationToken cancellationToken = default)
    {
        int fetchLimit = Math.Clamp(limit, 1, 100);
        var query = _dbContext.ShortUrls.AsNoTracking().OrderBy(x => x.Id);

        if (cursor.HasValue)
        {
            query = (IOrderedQueryable<ShortUrl>)query.Where(x => x.Id > cursor.Value);
        }

        // Lấy fetchLimit + 1 phần tử để xác định hasMore mà không tốn chi phí câu lệnh COUNT(*)
        var items = await query.Take(fetchLimit + 1).ToListAsync(cancellationToken);
        bool hasMore = items.Count > fetchLimit;
        var pagedItems = items.Take(fetchLimit).ToList();
        long? nextCursor = hasMore ? pagedItems[^1].Id : null;

        return (pagedItems, nextCursor, hasMore);
    }
}

