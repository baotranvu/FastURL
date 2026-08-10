using FastUrl.API.Middleware;
using FastUrl.Application.Infrastructure;
using FastUrl.Application.Interfaces;

using FastUrl.Domain.Common;
using FastUrl.Infrastructure.Persistence;
using FastUrl.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Đăng ký SwaggerGen để tự động tạo file đặc tả OpenAPI v1 JSON cho Scalar UI hiển thị
builder.Services.AddSwaggerGen();

// Đăng ký EF Core Database Provider động (Hỗ trợ chuyển đổi giữa PostgreSQL và SQLite từ Config/ENV)
string dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider", "PostgreSQL")!;
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Port=5432;Database=fasturl_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<FastUrlDbContext>(options =>
{
    if (dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString);
    }
    else if (dbProvider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        throw new InvalidOperationException($"Unsupported DatabaseProvider: '{dbProvider}'. Allowed values are 'PostgreSQL' or 'SQLite'.");
    }
});


// Đăng ký Scoped Repository Pattern
builder.Services.AddScoped<IShortUrlRepository, EfShortUrlRepository>();

// Đăng ký Singleton Service IShortCodeCodec -> Base62Codec (Thread-safe)
builder.Services.AddSingleton<IShortCodeCodec, Base62Codec>();

// Đọc WorkerId từ Cấu hình (appsettings.json hoặc Docker Environment Variable SnowflakeOptions__WorkerId)
long workerId = builder.Configuration.GetValue<long>("SnowflakeOptions:WorkerId", 1);
builder.Services.AddSingleton(new SnowflakeIdGenerator(workerId));

// Đăng ký Centralized Global Exception Handler .NET 8 (IExceptionHandler)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Đăng ký Redis IConnectionMultiplexer Singleton với AbortOnConnectFail = false (Fail-Open Resilience)
string redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
redisOptions.AbortOnConnectFail = false;
var redisMultiplexer = ConnectionMultiplexer.Connect(redisOptions);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);




var app = builder.Build();

// Tự động khởi tạo & Migrate Database Schema lên phiên bản mới nhất theo EF Core Migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FastUrlDbContext>();
    dbContext.Database.Migrate();
}


// Bật Giao Diện Scalar API Reference UI tại đường dẫn /scalar/v1 trong môi trường Development
if (app.Environment.IsDevelopment())
{
    // Cấu hình Swagger xuất file JSON đặc tả tại route /openapi/v1.json mà Scalar UI mặc định tìm kiếm
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });

    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Kích hoạt Middleware bắt lỗi toàn cục của .NET 8 (Tự động chuyển Exception thành RFC 7807 JSON)
app.UseExceptionHandler();

// Đăng ký UrlSafetyMiddleware bảo vệ an ninh cửa ngõ API (Layer 2 Security Guard)
app.UseMiddleware<UrlSafetyMiddleware>();

// Đăng ký SlidingWindowRateLimiterMiddleware bảo vệ chống DDoS & API Throttling bằng Redis ZSET (Layer 3 Distributed Throttling)
app.UseMiddleware<SlidingWindowRateLimiterMiddleware>();

app.UseAuthorization();
app.MapControllers();


app.Run();
