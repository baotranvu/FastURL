# =======================================================
# STAGE 1: BUILD STAGE (Dùng .NET 8 SDK Image để biên dịch)
# =======================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copy các file .csproj và restore trước để tận dụng Docker Layer Caching
COPY ["FastUrl.API/FastUrl.API.csproj", "FastUrl.API/"]
COPY ["FastUrl.Application/FastUrl.Application.csproj", "FastUrl.Application/"]
COPY ["FastUrl.Domain/FastUrl.Domain.csproj", "FastUrl.Domain/"]
COPY ["FastUrl.Infrastructure/FastUrl.Infrastructure.csproj", "FastUrl.Infrastructure/"]

RUN dotnet restore "FastUrl.API/FastUrl.API.csproj"

# 2. Copy toàn bộ mã nguồn và biên dịch Release
COPY . .
WORKDIR "/src/FastUrl.API"
RUN dotnet publish "FastUrl.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =======================================================
# STAGE 2: RUNTIME STAGE (Dùng ASP.NET 8 Runtime siêu nhẹ)
# =======================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Thiết lập cổng HTTP mặc định của Kestrel trong Container là 8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Copy sản phẩm đã biên dịch từ Stage 1 vào Stage 2
COPY --from=build /app/publish .

# Phân quyền ghi thư mục /app cho non-root user (app) tạo SQLite database file
RUN chown -R app:app /app

# Bảo mật: Chạy dưới quyền non-root user (app)
USER app

ENTRYPOINT ["dotnet", "FastUrl.API.dll"]
