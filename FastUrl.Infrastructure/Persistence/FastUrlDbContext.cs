using FastUrl.Domain.Entities;
using FastUrl.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FastUrl.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext cho ứng dụng FastUrl
/// </summary>
public class FastUrlDbContext : DbContext
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public FastUrlDbContext(DbContextOptions<FastUrlDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Nạp tự động tất cả IEntityTypeConfiguration trong Assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FastUrlDbContext).Assembly);
    }
}
