using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.Services;

public class PasswordPolicyServiceTests
{
    private readonly Mock<IAuthenticationSettingsService> _settings = new();

    private PasswordPolicyService CreateService() => new(_settings.Object);

    [Fact(DisplayName = "When password meets minimum length only, validation succeeds.")]
    public async Task ValidateAsync_MinLengthOnly_ReturnsValid()
    {
        // Arrange
        _settings.Setup(s => s.GetAuthenticationSettingsAsync()).ReturnsAsync(new AuthenticationSettings
        {
            PasswordMinLength = 8,
            RequireStrongPasswords = false
        });
        var service = CreateService();

        // Act
        var (isValid, message) = await service.ValidateAsync("abcdefgh", CancellationToken.None);

        // Assert
        isValid.ShouldBeTrue();
        message.ShouldBeEmpty();
    }

    [Fact(DisplayName = "When password is shorter than the configured minimum, validation fails.")]
    public async Task ValidateAsync_TooShort_ReturnsInvalid()
    {
        // Arrange
        _settings.Setup(s => s.GetAuthenticationSettingsAsync()).ReturnsAsync(new AuthenticationSettings
        {
            PasswordMinLength = 10,
            RequireStrongPasswords = false
        });
        var service = CreateService();

        // Act
        var (isValid, message) = await service.ValidateAsync("short", CancellationToken.None);

        // Assert
        isValid.ShouldBeFalse();
        message.ShouldContain("10");
    }

    [Fact(DisplayName = "When strong passwords are required and the password is weak, validation fails.")]
    public async Task ValidateAsync_StrongRequired_WeakPassword_ReturnsInvalid()
    {
        // Arrange
        _settings.Setup(s => s.GetAuthenticationSettingsAsync()).ReturnsAsync(new AuthenticationSettings
        {
            PasswordMinLength = 8,
            RequireStrongPasswords = true
        });
        var service = CreateService();

        // Act
        var (isValid, message) = await service.ValidateAsync("alllowercase", CancellationToken.None);

        // Assert
        isValid.ShouldBeFalse();
        message.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "When strong passwords are required and the password is complex, validation succeeds.")]
    public async Task ValidateAsync_StrongRequired_ComplexPassword_ReturnsValid()
    {
        // Arrange
        _settings.Setup(s => s.GetAuthenticationSettingsAsync()).ReturnsAsync(new AuthenticationSettings
        {
            PasswordMinLength = 8,
            RequireStrongPasswords = true
        });
        var service = CreateService();

        // Act
        var (isValid, message) = await service.ValidateAsync("GoodPass1!", CancellationToken.None);

        // Assert
        isValid.ShouldBeTrue();
        message.ShouldBeEmpty();
    }
}