using System;
using System.Linq;
using FastUrl.Application.Interfaces;
using FastUrl.Domain.Entities;
using Xunit;

namespace FastUrl.Domain.Tests.Architecture;

/// <summary>
/// Architecture Unit Tests tự động hóa kiểm tra tuân thủ nguyên lý Clean Architecture:
/// Sử dụng System.Reflection Native để đảm bảo zero-leak giữa các tầng!
/// </summary>
public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Application_Or_Infrastructure()
    {
        var domainAssembly = typeof(ShortUrl).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        Assert.DoesNotContain(referencedAssemblies, a => 
            a.Name != null && (a.Name.Contains("Application") || a.Name.Contains("Infrastructure") || a.Name.Contains("API")));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var applicationAssembly = typeof(IShortUrlRepository).Assembly;
        var referencedAssemblies = applicationAssembly.GetReferencedAssemblies();

        Assert.DoesNotContain(referencedAssemblies, a => 
            a.Name != null && (a.Name.Contains("Infrastructure") || a.Name.Contains("API")));
    }
}
