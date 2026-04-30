using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Features.ACL.Domain;
using Shouldly;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace PolyBucket.Tests.Features.Authentication.Services;

public class TokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly ITokenSettingsService _tokenSettingsService;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:Security:JwtSecret"] = "your-super-secret-key-with-at-least-32-characters-12345678",
                ["AppSettings:Security:JwtIssuer"] = "polybucket-api",
                ["AppSettings:Security:JwtAudience"] = "polybucket-client",
                ["AppSettings:Security:AccessTokenExpiryMinutes"] = "60",
                ["AppSettings:Security:RefreshTokenExpiryDays"] = "7"
            })
            .Build();

        _tokenSettingsService = new MockTokenSettingsService();
        _tokenService = new TokenService(_configuration, _tokenSettingsService);
    }

    [Fact(DisplayName = "When generating an access token, the token service creates a valid JWT containing the expected claims.")]
    public async Task GenerateAccessTokenAsync_ShouldCreateValidJwtWithCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            Role = new Role { Name = "User" }
        };

        // Act
        var token = await _tokenService.GenerateAccessTokenAsync(user);

        // Assert
        token.ShouldNotBeNullOrEmpty();

        // Decode the JWT token directly to verify all claims are present
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        // Verify all required claims are present in the JWT payload
        jwtToken.Claims.ShouldContain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        jwtToken.Claims.ShouldContain(c => c.Type == "email" && c.Value == user.Email);
        jwtToken.Claims.ShouldContain(c => c.Type == "name" && c.Value == user.Username);
        jwtToken.Claims.ShouldContain(c => c.Type == "role" && c.Value == user.Role.Name);
        jwtToken.Claims.ShouldContain(c => c.Type == "email_verified" && c.Value == "true");
        jwtToken.Claims.ShouldContain(c => c.Type == "jti");
        jwtToken.Claims.ShouldContain(c => c.Type == "iat");
        jwtToken.Claims.ShouldContain(c => c.Type == "exp");
        jwtToken.Claims.ShouldContain(c => c.Type == "iss" && c.Value == "polybucket-api");
        jwtToken.Claims.ShouldContain(c => c.Type == "aud" && c.Value == "polybucket-client");

        // Also validate using the TokenService method
        var principal = _tokenService.ValidateAccessToken(token);
        principal.ShouldNotBeNull();

        var claims = principal.Claims.ToList();
        
        // Check standard ASP.NET Core claim types
        claims.ShouldContain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        claims.ShouldContain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        claims.ShouldContain(c => c.Type == ClaimTypes.Name && c.Value == user.Username);
        claims.ShouldContain(c => c.Type == ClaimTypes.Role && c.Value == user.Role.Name);
    }

    [Fact(DisplayName = "When generating an access token, the token service creates a token with the correct expiration time.")]
    public async Task GenerateAccessTokenAsync_ShouldCreateTokenWithCorrectExpiration()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            Role = new Role { Name = "User" }
        };

        // Act
        var token = await _tokenService.GenerateAccessTokenAsync(user);

        // Assert
        var principal = _tokenService.ValidateAccessToken(token);
        principal.ShouldNotBeNull();

        var expClaim = principal.FindFirst("exp");
        expClaim.ShouldNotBeNull();

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
        var expectedExpiration = DateTimeOffset.UtcNow.AddMinutes(60);
        
        // Allow for a small time difference (within 1 minute)
        expirationTime.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        expirationTime.ShouldBeLessThanOrEqualTo(expectedExpiration);
    }

    [Fact(DisplayName = "When validating an invalid access token, the token service returns null.")]
    public void ValidateAccessToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var principal = _tokenService.ValidateAccessToken(invalidToken);

        // Assert
        principal.ShouldBeNull();
    }

    [Fact(DisplayName = "When generating an authentication response, the token service returns a complete response with tokens and user info.")]
    public async Task GenerateAuthenticationResponseAsync_ShouldReturnCompleteResponse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Role = new Role { Name = "User" },
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var response = await _tokenService.GenerateAuthenticationResponseAsync(user);

        // Assert
        response.ShouldNotBeNull();
        response.AccessToken.ShouldNotBeNullOrEmpty();
        response.RefreshToken.ShouldNotBeNullOrEmpty();
        response.User.ShouldNotBeNull();
        response.User.Id.ShouldBe(user.Id);
        response.User.Email.ShouldBe(user.Email);
        response.User.Username.ShouldBe(user.Username);
        response.User.Role.ShouldBe(user.Role.Name);
    }

    private class MockTokenSettingsService : ITokenSettingsService
    {
        public Task<TokenSettings> GetTokenSettingsAsync()
        {
            return Task.FromResult(new TokenSettings
            {
                AccessTokenExpiryHours = 1,
                RefreshTokenExpiryDays = 7,
                EnableRefreshTokens = true
            });
        }

        public Task<bool> UpdateTokenSettingsAsync(TokenSettings settings)
        {
            return Task.FromResult(true);
        }

        public Task<int> GetAccessTokenExpiryMinutesAsync()
        {
            return Task.FromResult(60);
        }

        public Task<int> GetRefreshTokenExpiryDaysAsync()
        {
            return Task.FromResult(7);
        }

        public Task<bool> IsRefreshTokensEnabledAsync()
        {
            return Task.FromResult(true);
        }
    }
} 