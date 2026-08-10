using FastUrl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FastUrl.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình Fluent API cho Thực thể ShortUrl trong Database:
/// 1. Tắt DB Identity tự tăng (Dùng Snowflake ID 64-bit sinh trên RAM C#)
/// 2. Đánh B-Tree Unique Index cho ShortCode tối ưu tốc độ truy vấn O(log N)
/// </summary>
public class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
{
    public void Configure(EntityTypeBuilder<ShortUrl> builder)
    {
        builder.ToTable("ShortUrls");

        // 1. Khóa chính PK: Snowflake 64-bit ID (Không dùng DB Identity tự tăng)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .ValueGeneratedNever()
               .IsRequired();

        // 2. B-Tree Unique Index cho ShortCode (Tối ưu O(log N) SELECT WHERE ShortCode = ...)
        builder.HasIndex(x => x.ShortCode)
               .IsUnique()
               .HasDatabaseName("IX_ShortUrls_ShortCode");

        builder.Property(x => x.ShortCode)
               .HasMaxLength(16)
               .IsRequired();

        builder.Property(x => x.OriginalUrl)
               .HasMaxLength(2048)
               .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
               .IsRequired();

        builder.Property(x => x.AccessCount)
               .IsRequired();
    }
}
