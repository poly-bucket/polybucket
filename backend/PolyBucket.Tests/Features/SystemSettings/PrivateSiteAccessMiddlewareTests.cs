using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Middleware;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings;

public class PrivateSiteAccessMiddlewareTests
{
    private readonly Mock<ISitePrivacyState> _privacy = new();

    private HttpContext BuildContext(string path, ClaimsPrincipal? user = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        context.User = user ?? new ClaimsPrincipal(new ClaimsIdentity());

        var provider = new Mock<IServiceProvider>();
        provider
            .Setup(p => p.GetService(typeof(ISitePrivacyState)))
            .Returns(_privacy.Object);
        context.RequestServices = provider.Object;

        return context;
    }

    private static ClaimsPrincipal AuthenticatedUser()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user") }, "Test");
        return new ClaimsPrincipal(identity);
    }

    [Fact(DisplayName = "When public browsing is allowed, the request passes through untouched.")]
    public async Task PublicSite_PassesThrough()
    {
        // Arrange
        _privacy.Setup(p => p.IsPublicBrowsingAllowedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var nextCalled = false;
        var middleware = new PrivateSiteAccessMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = BuildContext("/api/models");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        context.Response.Headers.ContainsKey(PrivateSiteAccessMiddleware.MarkerHeader).ShouldBeFalse();
    }

    [Fact(DisplayName = "When the site is private but the user is authenticated, the request passes through.")]
    public async Task PrivateSite_Authenticated_PassesThrough()
    {
        // Arrange
        _privacy.Setup(p => p.IsPublicBrowsingAllowedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var nextCalled = false;
        var middleware = new PrivateSiteAccessMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = BuildContext("/api/models", AuthenticatedUser());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Fact(DisplayName = "When the site is private and the request is anonymous to a gated path, it is blocked with 401 and the marker header.")]
    public async Task PrivateSite_AnonymousGatedPath_Blocks()
    {
        // Arrange
        _privacy.Setup(p => p.IsPublicBrowsingAllowedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var nextCalled = false;
        var middleware = new PrivateSiteAccessMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = BuildContext("/api/models");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeFalse();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        context.Response.Headers[PrivateSiteAccessMiddleware.MarkerHeader].ToString()
            .ShouldBe(PrivateSiteAccessMiddleware.MarkerValue);
    }

    [Theory(DisplayName = "When the site is private and anonymous, allowlisted paths still pass through.")]
    [InlineData("/api/auth/login")]
    [InlineData("/api/SystemSetup/status")]
    [InlineData("/api/system-settings/theme")]
    [InlineData("/health")]
    public async Task PrivateSite_AnonymousAllowlistedPath_PassesThrough(string path)
    {
        // Arrange
        _privacy.Setup(p => p.IsPublicBrowsingAllowedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var nextCalled = false;
        var middleware = new PrivateSiteAccessMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = BuildContext(path);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }
}
