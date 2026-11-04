using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Domain;
using PolyBucket.Api.Features.Authentication.Register.Domain;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Api.Tests;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.RecoveryCodeResetPassword;

[Collection("TestCollection")]
public class RecoveryCodePasswordResetIntegrationTests : BaseIntegrationTest
{
    public RecoveryCodePasswordResetIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When resetting a password with a valid recovery code, login succeeds with the new password and the code cannot be reused.")]
    public async Task ResetPasswordWithRecoveryCode_ValidCode_UpdatesPasswordAndConsumesCode()
    {
        // Arrange
        await ResetStateAsync();
        await SetPublicRegistrationAsync(allowed: true);
        const string email = "recovery-reset@test.com";
        const string originalPassword = "OriginalPass123!";
        const string newPassword = "NewSecurePass123!";

        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            username = "recoveryuser",
            password = originalPassword,
            confirmPassword = originalPassword
        });
        registerResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterCommandResponse>();
        registerBody.ShouldNotBeNull();
        registerBody!.RecoveryCodes.ShouldNotBeNull();
        registerBody.RecoveryCodes!.Count.ShouldBeGreaterThan(0);
        var recoveryCode = registerBody.RecoveryCodes[0];

        // Act
        var resetResponse = await Client.PostAsJsonAsync("/api/auth/recovery-code/reset-password", new RecoveryCodeResetPasswordCommand
        {
            EmailOrUsername = email,
            RecoveryCode = recoveryCode,
            NewPassword = newPassword
        });

        // Assert
        resetResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAuthToken(email, originalPassword)).ShouldBeEmpty();
        (await GetAuthToken(email, newPassword)).ShouldNotBeNullOrEmpty();

        var reuseResponse = await Client.PostAsJsonAsync("/api/auth/recovery-code/reset-password", new RecoveryCodeResetPasswordCommand
        {
            EmailOrUsername = email,
            RecoveryCode = recoveryCode,
            NewPassword = "AnotherPass123!"
        });
        reuseResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task SetPublicRegistrationAsync(bool allowed)
    {
        if (!await DbContext.SystemSetups.AnyAsync())
        {
            DbContext.SystemSetups.Add(new SystemSetup
            {
                Id = Guid.NewGuid(),
                AllowUserRegistration = allowed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var setup = await DbContext.SystemSetups.FirstAsync();
            setup.AllowUserRegistration = allowed;
        }

        await DbContext.SaveChangesAsync();
    }
}
