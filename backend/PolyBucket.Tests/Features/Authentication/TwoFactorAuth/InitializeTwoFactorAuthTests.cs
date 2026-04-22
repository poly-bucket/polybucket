using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository;
using PolyBucket.Tests;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.TwoFactorAuth
{
    [Collection("TestCollection")]
    public class InitializeTwoFactorAuthTests : BaseIntegrationTest
    {
        private readonly PolyBucketDbContext _context;
        private readonly IInitializeTwoFactorAuthRepository _repository;

        public InitializeTwoFactorAuthTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _context = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            _repository = ServiceScope.ServiceProvider.GetRequiredService<IInitializeTwoFactorAuthRepository>();
        }

        [Fact]
        public async Task Initialize_WithValidUser_ShouldInitializeSuccessfully()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            var command = new InitializeTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/initialize", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<InitializeTwoFactorAuthResponse>();
            result.ShouldNotBeNull();
            result.QrCodeUrl.ShouldNotBeNullOrEmpty();
            result.SecretKey.ShouldNotBeNullOrEmpty();
            result.SecretKey.Length.ShouldBe(16); // Base32 encoded secret key

            // Verify 2FA is created in database (but not enabled)
            var twoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            twoFactorAuth.ShouldNotBeNull();
            twoFactorAuth.IsEnabled.ShouldBeFalse();
            twoFactorAuth.SecretKey.ShouldBe(result.SecretKey);
        }

        [Fact]
        public async Task Initialize_WithAlreadyInitializedUser_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create already initialized 2FA
            var existingTwoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                EnabledAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 1
            };
            await _context.TwoFactorAuths.AddAsync(existingTwoFactorAuth);
            await _context.SaveChangesAsync();

            var command = new InitializeTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/initialize", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.ShouldContain("already initialized");
        }

        [Fact]
        public async Task Initialize_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new InitializeTwoFactorAuthCommand { UserId = Guid.NewGuid() };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/initialize", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Initialize_WithDifferentUserInToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var user1 = await CreateTestUser("user1@test.com");
            var user2 = await CreateTestUser("user2@test.com");
            var token = await GetAuthToken(user1.Email, "TestPassword123!");
            var command = new InitializeTwoFactorAuthCommand { UserId = user2.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/initialize", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Initialize_ShouldCreateNewTwoFactorAuthRecord()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            var command = new InitializeTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/initialize", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            // Verify new record is created
            var twoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            twoFactorAuth.ShouldNotBeNull();
            twoFactorAuth.IsEnabled.ShouldBeFalse();
            twoFactorAuth.EnabledAt.ShouldBeNull();
            twoFactorAuth.SecretKey.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task Initialize_WithUnenabledExistingTwoFactorAuth_ShouldReplaceIt()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create unenabled 2FA
            var existingTwoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = "OLD_SECRET_KEY",
                IsEnabled = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 1
            };
            await _context.TwoFactorAuths.AddAsync(existingTwoFactorAuth);
            await _context.SaveChangesAsync();

            var command = new InitializeTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/initialize", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            // Verify old record is replaced
            var twoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            twoFactorAuth.ShouldNotBeNull();
            twoFactorAuth.SecretKey.ShouldNotBe("OLD_SECRET_KEY");
            twoFactorAuth.IsEnabled.ShouldBeFalse();
        }
    }
} 