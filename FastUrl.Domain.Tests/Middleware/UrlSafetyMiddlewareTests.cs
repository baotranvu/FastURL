using System.IO;
using System.Text;
using System.Threading.Tasks;
using FastUrl.Application.Middleware;
using FastUrl.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace FastUrl.Domain.Tests.Middleware;

public class UrlSafetyMiddlewareTests
{
    [Fact]
    public async Task Invoke_GetRequest_ShouldPassToNextMiddleware()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/5fiF";

        bool isNextCalled = false;
        var middleware = new UrlSafetyMiddleware((ctx) =>
        {
            isNextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Invoke_ValidHttpsUrl_ShouldPassToNextMiddleware()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/v1/urls";

        string jsonPayload = @"{""url"": ""https://google.com/search?q=dotnet""}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonPayload));

        bool isNextCalled = false;
        var middleware = new UrlSafetyMiddleware((ctx) =>
        {
            isNextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(isNextCalled);
    }

    [Theory]
    [InlineData(@"javascript:alert(1)")]
    [InlineData(@"file:///C:/Windows/System32/drivers/etc/hosts")]
    [InlineData(@"ftp://ftp.example.com/file.txt")]
    public async Task Invoke_MaliciousScheme_ShouldThrowSecurityViolationException(string invalidUrl)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/v1/urls";

        string jsonPayload = $"{{\"url\": \"{invalidUrl}\"}}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonPayload));

        bool isNextCalled = false;
        var middleware = new UrlSafetyMiddleware((ctx) =>
        {
            isNextCalled = true;
            return Task.CompletedTask;
        });

        await Assert.ThrowsAsync<SecurityViolationException>(() => middleware.InvokeAsync(context));
        Assert.False(isNextCalled);
    }

    [Theory]
    [InlineData(@"http://localhost/5fiF")]
    [InlineData(@"http://127.0.0.1:5000/xyz")]
    [InlineData(@"https://fasturl.api/redirect")]
    public async Task Invoke_SelfLoopDomain_ShouldThrowSecurityViolationException(string selfLoopUrl)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/v1/urls";

        string jsonPayload = $"{{\"url\": \"{selfLoopUrl}\"}}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonPayload));

        bool isNextCalled = false;
        var middleware = new UrlSafetyMiddleware((ctx) =>
        {
            isNextCalled = true;
            return Task.CompletedTask;
        });

        await Assert.ThrowsAsync<SecurityViolationException>(() => middleware.InvokeAsync(context));
        Assert.False(isNextCalled);
    }
}
