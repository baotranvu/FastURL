# 🏛️ FastUrl.API Architecture & Specification Document

## 🌐 Overview
**FastUrl.API** is a high-performance, enterprise-grade, distributed URL Shortener Web API built on **.NET 8 Web API** and **Clean Architecture**.

---

## 📐 System Architecture Layers

```
 [Presentation API Gateway Layer]  ──► FastUrl.API (Scalar UI, UrlController, Security Middleware)
              │
              ▼
   [Application Use Case Layer]    ──► FastUrl.Application (Interfaces, Middleware, Global Exception Handler)
              │
              ▼
    [Core Domain Logic Layer]      ──► FastUrl.Domain (Base62Codec, SnowflakeIdGenerator, ShortUrl Entity)
              │
              ▼
   [Infrastructure Data Layer]     ──► FastUrl.Infrastructure (EF Core, FastUrlDbContext, B-Tree Unique Index)
```

---

## ⚡ Core Technical Capabilities & Benchmarks

1. **Base62 Encoding Engine**:
   * Uses `stackalloc char[11]` zero-heap allocation logic for encoding.
   * Uses $O(1)$ Direct ASCII Lookup Map (`int[128]`) with Horner's Rule polynomial decoding.
2. **Twitter Snowflake 64-Bit ID Generator**:
   * Tailored Bit Layout: `[1-bit Sign (0)] [41-bits Timestamp (ms)] [3-bits Worker ID] [19-bits Sequence Counter]`.
   * Single-node sequence capacity: **524,288 IDs / millisecond** ($\approx 524 \text{ Million IDs/sec}$).
   * Supports up to 8 distributed container nodes (`WorkerId = 0..7`).
3. **Layer 2 Security Guard (`UrlSafetyMiddleware`)**:
   * Early-return route filter (`POST /api/v1/urls`).
   * Stream rewind buffering via `EnableBuffering()`.
   * Scheme whitelist (`http://` and `https://` strictly enforced).
   * Self-loop prevention (blocks shortening `localhost`, `127.0.0.1`, `fasturl.api`).
4. **Centralized Error Handling (.NET 8 `IExceptionHandler`)**:
   * Automatic mapping of exceptions (`SecurityViolationException`) into international **RFC 7807 Problem Details** JSON format.
5. **Database Persistence & B-Tree Indexing**:
   * Primary Key: `ValueGeneratedNever()` (Preserves Snowflake 64-bit ID generated in C# memory).
   * Unique B-Tree Index on `ShortCode` (`IX_ShortUrls_ShortCode`) for $O(\log N)$ sub-millisecond lookup.
6. **API Visualization & Containerization**:
   * **Scalar API Reference UI** at `/scalar/v1` fed via OpenAPI specification `/openapi/v1.json`.
   * **Multi-Stage Dockerization** (`Dockerfile`, `.dockerignore`, `docker-compose.yml`).
7. **Automated Architecture Enforcement (`NetArchTest.eNet`)**:
   * Continuous integration unit tests enforcing zero layer leaks across Domain, Application, Infrastructure, and API.
