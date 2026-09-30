using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Authorization;
using PolyBucket.Api.Features.Authentication.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Authorization;

public class RequireVerifiedEmailAttributeTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly Mock<IEmailSettingsResolver> _resolver = new();

    public RequireVerifiedEmailAttributeTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        SetupSettings(requireVerification: true);
    }

    public void Dispose() => _context.Dispose();

    private void SetupSettings(bool requireVerification, bool canDeliver = true)
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(canDeliver
                ? new EffectiveEmailSettings
                {
                    Transport = EmailTransportKind.Log,
                    FromAddress = "noreply@example.com",
                    PublicBaseUrl = "https://models.example.com",
                    RequireEmailVerification = requireVerification
                }
                : new EffectiveEmailSettings { RequireEmailVerification = requireVerification });
    }

    private AuthorizationFilterContext CreateContext(Guid userId, string? emailVerifiedClaim)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (emailVerifiedClaim != null)
        {
            claims.Add(new Claim(TokenService.EmailVerifiedClaim, emailVerifiedClaim));
        }

        var services = new ServiceCollection();
        services.AddSingleton<IEmailVerificationGate>(new EmailVerificationGate(_resolver.Object, _context));
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")),
            RequestServices = services.BuildServiceProvider()
        };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact(DisplayName = "When verification is required and the token says unverified, the request is rejected with the email_unverified code.")]
    public async Task OnAuthorization_UnverifiedUser_ShouldReturn403WithCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User { Id = userId, Email = "user@example.com", Username = "user", Salt = "salt", PasswordHash = "hash" });
        await _context.SaveChangesAsync();
        var context = CreateContext(userId, "false");

        // Act
        await new RequireVerifiedEmailAttribute().OnAuthorizationAsync(context);

        // Assert
        var result = context.Result.ShouldBeOfType<ObjectResult>();
        result.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
        var problem = result.Value.ShouldBeOfType<ProblemDetails>();
        problem.Extensions["code"].ShouldBe(RequireVerifiedEmailAttribute.ErrorCode);
    }

    [Fact(DisplayName = "When the token claims verified, the request is allowed without a database lookup.")]
    public async Task OnAuthorization_VerifiedClaim_ShouldAllow()
    {
        // Arrange
        var context = CreateContext(Guid.NewGuid(), "true");

        // Act
        await new RequireVerifiedEmailAttribute().OnAuthorizationAsync(context);

        // Assert
        context.Result.ShouldBeNull();
    }

    [Fact(DisplayName = "When the access token is stale but the user has since verified, the database check allows the request.")]
    public async Task OnAuthorization_StaleClaimButVerifiedInDatabase_ShouldAllow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User { Id = userId, Email = "user@example.com", Username = "user", Salt = "salt", PasswordHash = "hash", EmailVerifiedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        var context = CreateContext(userId, "false");

        // Act
        await new RequireVerifiedEmailAttribute().OnAuthorizationAsync(context);

        // Assert
        context.Result.ShouldBeNull();
    }

    [Fact(DisplayName = "When the setting is off, unverified users are allowed.")]
    public async Task OnAuthorization_SettingOff_ShouldAllow()
    {
        // Arrange
        SetupSettings(requireVerification: false);
        var context = CreateContext(Guid.NewGuid(), "false");

        // Act
        await new RequireVerifiedEmailAttribute().OnAuthorizationAsync(context);

        // Assert
        context.Result.ShouldBeNull();
    }

    [Fact(DisplayName = "When email cannot be delivered, the gate stays open so users are never locked out by a broken mail server.")]
    public async Task OnAuthorization_EmailCannotDeliver_ShouldAllow()
    {
        // Arrange
        SetupSettings(requireVerification: true, canDeliver: false);
        var context = CreateContext(Guid.NewGuid(), "false");

        // Act
        await new RequireVerifiedEmailAttribute().OnAuthorizationAsync(context);

        // Assert
        context.Result.ShouldBeNull();
    }
}
