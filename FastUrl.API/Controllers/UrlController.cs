using System;
using System.Threading;
using System.Threading.Tasks;
using FastUrl.Application.Interfaces;
using FastUrl.Domain.Common;
using FastUrl.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FastUrl.API.Controllers;

[ApiController]
public class UrlController : ControllerBase
{
    private readonly IShortCodeCodec _codec;
    private readonly SnowflakeIdGenerator _snowflakeIdGenerator;
    private readonly IShortUrlRepository _repository;

    public UrlController(
        IShortCodeCodec codec,
        SnowflakeIdGenerator snowflakeIdGenerator,
        IShortUrlRepository repository)
    {
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        _snowflakeIdGenerator = snowflakeIdGenerator ?? throw new ArgumentNullException(nameof(snowflakeIdGenerator));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Rút gọn link (Tạo short code từ original URL và lưu vào Database vĩnh viễn)
    /// POST /api/v1/urls
    /// </summary>
    [HttpPost("api/v1/urls")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "URL cannot be empty." });
        }

        // 1. Sinh ID 64-bit chuẩn Twitter Snowflake ID (3-bit Worker, 19-bit Sequence)
        long id = _snowflakeIdGenerator.NextId();

        // 2. Mã hóa ID sang chuỗi Base62 shortCode (dùng Base62Codec stackalloc 0-Byte Heap)
        string shortCode = _codec.Encode(id);

        // 3. Khởi tạo Domain Entity
        var entity = new ShortUrl(id, shortCode, request.Url);

        // 4. Lưu vào Database vĩnh viễn qua EF Core Repository (B-Tree Unique Index)
        await _repository.AddAsync(entity, cancellationToken);

        // 5. Dựng URL ngắn hoàn chỉnh có thể click được
        string shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}";

        // 6. Trả về Response HTTP 201 Created
        return Created($"/{shortCode}", new
        {
            id = entity.Id,
            shortCode = entity.ShortCode,
            originalUrl = entity.OriginalUrl,
            shortUrl,
            createdAtUtc = entity.CreatedAtUtc
        });
    }

    /// <summary>
    /// Redirect từ mã ngắn về original URL (HTTP 302 Found) sử dụng B-Tree Index
    /// GET /{shortCode}
    /// </summary>
    [HttpGet("{shortCode}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectUrl(string shortCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return BadRequest(new { error = "Short code cannot be empty." });
        }

        // 1. Truy vấn nhanh O(log N) từ Database nhờ B-Tree Unique Index trên ShortCode
        var shortUrlEntity = await _repository.GetByShortCodeAsync(shortCode, cancellationToken);

        if (shortUrlEntity != null)
        {
            // Tăng số lượt access bất đồng bộ (Analytics)
            _ = _repository.IncrementAccessCountAsync(shortCode, cancellationToken);

            // Trả về HTTP 302 Found Redirect về URL gốc
            return Redirect(shortUrlEntity.OriginalUrl);
        }

        // 2. Không tìm thấy mã ngắn
        return NotFound(new { error = $"Short code '{shortCode}' not found." });
    }

    /// <summary>
    /// Phân trang dữ liệu ShortUrls theo thuật toán con trỏ Cursor Seek O(log N)
    /// GET /api/v1/urls?cursor=79048694150201344&limit=20
    /// </summary>
    [HttpGet("api/v1/urls")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPagedUrls(
        [FromQuery] long? cursor,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        int safeLimit = Math.Clamp(limit, 1, 100);
        var (items, nextCursor, hasMore) = await _repository.GetPagedAsync(cursor, safeLimit, cancellationToken);

        return Ok(new
        {
            data = System.Linq.Enumerable.Select(items, x => new
            {
                id = x.Id,
                shortCode = x.ShortCode,
                originalUrl = x.OriginalUrl,
                shortUrl = $"{Request.Scheme}://{Request.Host}/{x.ShortCode}",
                createdAtUtc = x.CreatedAtUtc,
                accessCount = x.AccessCount
            }),
            nextCursor,
            hasMore,
            limit = safeLimit
        });
    }
}


public record ShortenUrlRequest(string Url);
