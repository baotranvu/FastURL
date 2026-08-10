# FastUrl API (.NET 9)

A clean architecture implementation of a URL Shortener API, built with ASP.NET Core Web API.

## Project Structure

This solution manually implements Clean Architecture with the following project separation:

- **FastUrl.Domain**: Core business logic, entities, value objects, and repository interfaces. Has zero external dependencies.
- **FastUrl.Application**: Use cases, CQRS commands/queries, request handlers, and DTOs. Depends on `Domain`.
- **FastUrl.Infrastructure**: Database access (EF Core), migrations, external API integrations, caching. Depends on `Application`.
- **FastUrl.API**: Presentation layer, controllers, routes, middleware, and dependency injection registration. Depends on `Infrastructure` and `Application`.
- **FastUrl.Domain.Tests**: Unit tests for domain logic and core algorithms. Depends on `Domain`.

---

## Database Indexing Strategy (C# vs. Laravel)

To ensure \(O(1)\) lookup time when redirecting a short URL code to its original URL, we must define a unique index on the `ShortCode` column.

### 1. C# Entity Framework Core (Fluent API)
In the Infrastructure layer (`DbContext` configurations), we map the entity index using Fluent API inside `OnModelCreating` or a specific entity configuration class:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FastUrl.Domain.Entities;

public class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
{
    public void Configure(EntityTypeBuilder<ShortUrl> builder)
    {
        // Define primary key
        builder.HasKey(x => x.Id);

        // Define Unique Index on ShortCode to optimize redirects
        builder.HasIndex(x => x.ShortCode)
               .IsUnique()
               .HasDatabaseName("IX_ShortUrls_ShortCode");
    }
}
```

### 2. Laravel Eloquent Migrations
For comparison, in a Laravel project, we would define the same unique constraint in the migration schema:

```php
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

Schema::create('short_urls', function (Blueprint $table) {
    $table->id();
    $table->string('original_url');
    
    // Define unique column which automatically creates a unique index
    $table->string('short_code')->unique(); 
    
    $table->timestamps();
});
```

---

## How to Run & Build

1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Build solution:
   ```bash
   dotnet build
   ```
3. Run test suite:
   ```bash
   dotnet test
   ```
