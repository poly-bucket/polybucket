using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Domain;
using PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GeneratePasswordResetLink;

public class GeneratePasswordResetLinkServiceTests
{
    private readonly Mock<IGeneratePasswordResetLinkRepository> _repository = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly User _user = new() { Id = Guid.NewGuid(), Email = "user@example.com", Username = "user" };

    public GeneratePasswordResetLinkServiceTests()
    {
        _tokenService.Setup(t => t.GeneratePasswordResetToken()).Returns("admin-raw-token");
        _repository.Setup(r => r.FindByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
    }

    private GeneratePasswordResetLinkService CreateService(string publicBaseUrl)
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings { PublicBaseUrl = publicBaseUrl });
        return new GeneratePasswordResetLinkService(_repository.Object, _tokenService.Object, _resolver.Object, TimeProvider.System, NullLogger<GeneratePasswordResetLinkService>.Instance);
    }

    [Fact(DisplayName = "When generating a link, only the token hash is stored, the action is audited, and the absolute URL is returned.")]
    public async Task Generate_WithPublicBaseUrl_ShouldReturnAbsoluteUrlAndStoreHash()
    {
        // Arrange
        var service = CreateService("https://models.example.com");
        var adminId = Guid.NewGuid();

        // Act
        var result = await service.GenerateAsync(_user.Id, adminId, new ClientRequestInfo("10.0.0.2", "tests"));

        // Assert
        result.Path.ShouldBe("/reset-password?token=admin-raw-token");
        result.Url.ShouldBe("https://models.example.com/reset-password?token=admin-raw-token");
        result.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow.AddHours(23));
        _repository.Verify(r => r.AddPasswordResetToken(It.Is<PasswordResetToken>(t =>
            t.Token == TokenHasher.Hash("admin-raw-token") && t.Email == _user.Email)), Times.Once);
        _repository.Verify(r => r.AddAuditLog(It.Is<UserAuditLog>(a =>
            a.UserId == _user.Id && a.PerformedByUserId == adminId && a.Action == UserAuditAction.PasswordResetLinkGenerated)), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When no public base URL is configured, only the relative path is returned.")]
    public async Task Generate_WithoutPublicBaseUrl_ShouldReturnPathOnly()
    {
        // Arrange
        var service = CreateService(string.Empty);

        // Act
        var result = await service.GenerateAsync(_user.Id, Guid.NewGuid(), ClientRequestInfo.Unknown);

        // Assert
        result.Url.ShouldBeNull();
        result.Path.ShouldStartWith("/reset-password?token=");
    }

    [Fact(DisplayName = "When the user does not exist, the service throws NotFoundException.")]
    public async Task Generate_UnknownUser_ShouldThrowNotFound()
    {
        // Arrange
        var service = CreateService("https://models.example.com");

        // Act
        var act = () => service.GenerateAsync(Guid.NewGuid(), Guid.NewGuid(), ClientRequestInfo.Unknown);

        // Assert
        await Should.ThrowAsync<NotFoundException>(act);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
