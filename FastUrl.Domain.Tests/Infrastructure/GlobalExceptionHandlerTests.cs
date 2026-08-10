using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FastUrl.Application.Infrastructure;
using FastUrl.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace FastUrl.Domain.Tests.Infrastructure;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_SecurityViolationException_ShouldReturn400BadRequest()
    {
        var handler = new GlobalExceptionHandler();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/urls";
        context.Response.Body = new MemoryStream();

        var exception = new SecurityViolationException("Security violation: Only HTTP and HTTPS URLs are allowed.");

        bool isHandled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(isHandled);
        Assert.Equal(400, context.Response.StatusCode);
        Assert.Contains("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_ShouldReturn500InternalServerError()
    {
        var handler = new GlobalExceptionHandler();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/urls";
        context.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException("Unexpected database failure.");

        bool isHandled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(isHandled);
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Contains("application/json", context.Response.ContentType);
    }
}
