using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication;

[Collection("TestCollection")]
public class RequestSecurityIntegrationTests : BaseIntegrationTest
{
    public RequestSecurityIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When a user registers, the response body never contains an email verification token.")]
    public async Task Register_ResponseBody_DoesNotContainVerificationToken()
    {
        // Arrange
        await ResetStateAsync();
        var payload = new
        {
            email = "no-token-leak@test.com",
            username = "notokenleak",
            password = "Password123!",
            confirmPassword = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain("emailVerificationToken", Case.Insensitive);
    }

    [Fact(DisplayName = "When the strict auth rate limit is exceeded, the login endpoint returns 429 with a Retry-After header.")]
    public async Task Login_ExceedingStrictLimit_Returns429()
    {
        // Arrange
        await ResetStateAsync();
        await using var limitedFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimiting:Enabled", "true");
            builder.UseSetting("RateLimiting:AuthStrict:PermitLimit", "2");
            builder.UseSetting("RateLimiting:AuthStrict:WindowSeconds", "60");
        });
        var client = limitedFactory.CreateClient();
        var command = new LoginCommand { EmailOrUsername = "nobody@test.com", Password = "wrong-password" };

        // Act
        var first = await client.PostAsJsonAsync("/api/auth/login", command);
        var second = await client.PostAsJsonAsync("/api/auth/login", command);
        var third = await client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        first.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        second.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        third.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        third.Headers.Contains("Retry-After").ShouldBeTrue();
    }

    [Fact(DisplayName = "When a trusted proxy forwards a client IP, login records and refresh tokens store the forwarded IP.")]
    public async Task Login_BehindTrustedProxy_StoresForwardedClientIp()
    {
        // Arrange
        await ResetStateAsync();
        var user = await CreateTestUser("forwarded-ip@test.com");
        await using var proxiedFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services => services.AddSingleton<IStartupFilter, LoopbackRemoteIpStartupFilter>());
        });
        var client = proxiedFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginCommand { EmailOrUsername = user.Email, Password = "TestPassword123!" })
        };
        request.Headers.Add("X-Forwarded-For", "203.0.113.7");
        request.Headers.UserAgent.ParseAdd("PolyBucketTests/1.0");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        DbContext.ChangeTracker.Clear();
        var refreshToken = await DbContext.RefreshTokens.Where(t => t.UserId == user.Id).OrderByDescending(t => t.CreatedAt).FirstAsync();
        refreshToken.CreatedByIp.ShouldBe("203.0.113.7");
        var loginRecord = await DbContext.UserLogins.Where(l => l.UserId == user.Id).OrderByDescending(l => l.CreatedAt).FirstAsync();
        loginRecord.IpAddress.ShouldBe("203.0.113.7");
        loginRecord.UserAgent.ShouldContain("PolyBucketTests");
    }

    private sealed class LoopbackRemoteIpStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    context.Connection.RemoteIpAddress = IPAddress.Loopback;
                    await nextMiddleware();
                });
                next(app);
            };
        }
    }
}
