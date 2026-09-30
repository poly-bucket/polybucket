using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Authentication.Authorization;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication;

[Collection("TestCollection")]
public class EmailVerificationFlowIntegrationTests : BaseIntegrationTest
{
    private static readonly Regex TokenPattern = new("token=([A-Za-z0-9%_\\-]+)", RegexOptions.Compiled);

    public EmailVerificationFlowIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private async Task EnableEmailAsync(bool requireVerification)
    {
        DbContext.SystemSettings.AddRange(
            new SystemSetting { Key = SystemSettingKeys.EmailTransport, Value = nameof(EmailTransportKind.Log) },
            new SystemSetting { Key = SystemSettingKeys.EmailFromAddress, Value = "noreply@polybucket.test" },
            new SystemSetting { Key = SystemSettingKeys.EmailPublicBaseUrl, Value = "https://models.polybucket.test" },
            new SystemSetting { Key = SystemSettingKeys.EmailRequireVerification, Value = requireVerification ? "true" : "false" });
        await DbContext.SaveChangesAsync();
        ServiceScope.ServiceProvider.GetRequiredService<IEmailSettingsResolver>().Invalidate();
        Factory.EmailTransport.Clear();
    }

    private async Task DispatchAsync()
    {
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IEmailDispatcher>().DispatchPendingAsync();
    }

    private string ExtractTokenFromEmailTo(string recipient, string path)
    {
        var email = Factory.EmailTransport.SentTo(recipient).Last();
        email.TextBody.ShouldContain($"https://models.polybucket.test{path}?token=");
        var match = TokenPattern.Match(email.TextBody);
        match.Success.ShouldBeTrue();
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }

    private static string ReadString(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var segment in path)
        {
            current = current.EnumerateObject()
                .First(p => p.Name.Equals(segment, StringComparison.OrdinalIgnoreCase))
                .Value;
        }

        return current.GetString() ?? string.Empty;
    }

    private async Task<(string AccessToken, string RefreshToken)> RegisterAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            username = email.Split('@')[0],
            password = "Password123!",
            confirmPassword = "Password123!"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (ReadString(doc.RootElement, "authentication", "accessToken"), ReadString(doc.RootElement, "authentication", "refreshToken"));
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpMethod method, string url, string accessToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await Client.SendAsync(request);
    }

    [Fact(DisplayName = "When a new user registers, verifies from the emailed link, and reloads their profile, the account is reported as verified.")]
    public async Task Register_Dispatch_Verify_Me_ReportsVerified()
    {
        // Arrange
        await ResetStateAsync();
        await EnableEmailAsync(requireVerification: true);
        var (accessToken, _) = await RegisterAsync("new-maker@polybucket.test");
        await DispatchAsync();
        var rawToken = ExtractTokenFromEmailTo("new-maker@polybucket.test", "/verify-email");

        // Act
        var verify = await Client.PostAsJsonAsync("/api/auth/verify-email", new { token = rawToken });
        var me = await SendAuthorizedAsync(HttpMethod.Get, "/api/auth/me", accessToken);

        // Assert
        verify.StatusCode.ShouldBe(HttpStatusCode.OK);
        me.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("isEmailVerified").GetBoolean().ShouldBeTrue();
        var stored = await DbContext.EmailVerificationTokens.AsNoTracking().SingleAsync();
        stored.Token.ShouldBe(TokenHasher.Hash(rawToken));
        stored.Token.ShouldNotBe(rawToken);
        stored.IsUsed.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the same verification link is submitted twice, the second attempt is rejected.")]
    public async Task VerifyEmail_ReusedLink_IsRejected()
    {
        // Arrange
        await ResetStateAsync();
        await EnableEmailAsync(requireVerification: true);
        await RegisterAsync("twice@polybucket.test");
        await DispatchAsync();
        var rawToken = ExtractTokenFromEmailTo("twice@polybucket.test", "/verify-email");
        await Client.PostAsJsonAsync("/api/auth/verify-email", new { token = rawToken });

        // Act
        var second = await Client.PostAsJsonAsync("/api/auth/verify-email", new { token = rawToken });

        // Assert
        second.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "When verification is required, an unverified user is blocked from creating content with the email_unverified code, and allowed right after verifying even with the old access token.")]
    public async Task UnverifiedUser_IsBlockedUntilVerified()
    {
        // Arrange
        await ResetStateAsync();
        await EnableEmailAsync(requireVerification: true);
        var (accessToken, _) = await RegisterAsync("blocked@polybucket.test");
        await DispatchAsync();
        var rawToken = ExtractTokenFromEmailTo("blocked@polybucket.test", "/verify-email");

        // Act
        var beforeVerify = await SendAuthorizedAsync(HttpMethod.Post, "/api/collections", accessToken, new { name = "My prints" });
        await Client.PostAsJsonAsync("/api/auth/verify-email", new { token = rawToken });
        var afterVerify = await SendAuthorizedAsync(HttpMethod.Post, "/api/collections", accessToken, new { name = "My prints" });

        // Assert
        beforeVerify.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await beforeVerify.Content.ReadAsStringAsync()).ShouldContain(RequireVerifiedEmailAttribute.ErrorCode);
        afterVerify.StatusCode.ShouldNotBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When verification is not required, unverified users can still create content.")]
    public async Task UnverifiedUser_NotRequired_IsAllowed()
    {
        // Arrange
        await ResetStateAsync();
        await EnableEmailAsync(requireVerification: false);
        var (accessToken, _) = await RegisterAsync("relaxed@polybucket.test");

        // Act
        var response = await SendAuthorizedAsync(HttpMethod.Post, "/api/collections", accessToken, new { name = "My prints" });

        // Assert
        response.StatusCode.ShouldNotBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "When a user resets their password from the emailed link, existing refresh tokens stop working and the new password signs in.")]
    public async Task ForgotPassword_Reset_RevokesOldSessions()
    {
        // Arrange
        await ResetStateAsync();
        await EnableEmailAsync(requireVerification: false);
        var user = await CreateTestUser("forgetful@polybucket.test", "OldPassword123!");
        await DbContext.SaveChangesAsync();
        var login = await Client.PostAsJsonAsync("/api/auth/login", new { emailOrUsername = user.Email, password = "OldPassword123!" });
        using var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var oldRefreshToken = ReadString(loginDoc.RootElement, "refreshToken");
        await Client.PostAsJsonAsync("/api/auth/forgot-password", new { email = user.Email });
        await DispatchAsync();
        var rawToken = ExtractTokenFromEmailTo(user.Email, "/reset-password");

        // Act
        var reset = await Client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            token = rawToken,
            newPassword = "NewPassword123!",
            confirmPassword = "NewPassword123!"
        });
        await DispatchAsync();
        var refreshWithOld = await Client.PostAsJsonAsync("/api/auth/refresh-token", new { refreshToken = oldRefreshToken });
        var loginWithNew = await Client.PostAsJsonAsync("/api/auth/login", new { emailOrUsername = user.Email, password = "NewPassword123!" });
        var reuse = await Client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            token = rawToken,
            newPassword = "Another123!",
            confirmPassword = "Another123!"
        });

        // Assert
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        oldRefreshToken.ShouldNotBeNullOrEmpty();
        reset.StatusCode.ShouldBe(HttpStatusCode.OK);
        refreshWithOld.IsSuccessStatusCode.ShouldBeFalse();
        loginWithNew.StatusCode.ShouldBe(HttpStatusCode.OK);
        reuse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var storedTokens = await DbContext.PasswordResetTokens.AsNoTracking().ToListAsync();
        storedTokens.ShouldAllBe(t => t.Token != rawToken);
        Factory.EmailTransport.SentTo(user.Email).ShouldContain(m => m.Subject.Contains("password", StringComparison.OrdinalIgnoreCase) && !m.TextBody.Contains("token="));
    }

    [Fact(DisplayName = "When a verification link is resent, the previous link stops working and the new one verifies the account.")]
    public async Task ResendVerification_InvalidatesPreviousLink()
    {
        // Arrange
        await ResetStateAsync();
        await EnableEmailAsync(requireVerification: true);
        await RegisterAsync("resend@polybucket.test");
        await DispatchAsync();
        var firstToken = ExtractTokenFromEmailTo("resend@polybucket.test", "/verify-email");
        await DbContext.EmailVerificationTokens.ExecuteUpdateAsync(s => s.SetProperty(t => t.CreatedAt, DateTime.UtcNow.AddMinutes(-5)));

        // Act
        var resend = await Client.PostAsJsonAsync("/api/auth/verify-email/resend", new { email = "resend@polybucket.test" });
        await DispatchAsync();
        var secondToken = ExtractTokenFromEmailTo("resend@polybucket.test", "/verify-email");
        var withFirst = await Client.PostAsJsonAsync("/api/auth/verify-email", new { token = firstToken });
        var withSecond = await Client.PostAsJsonAsync("/api/auth/verify-email", new { token = secondToken });

        // Assert
        resend.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        secondToken.ShouldNotBe(firstToken);
        withFirst.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        withSecond.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
