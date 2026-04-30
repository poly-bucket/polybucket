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
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository;
using PolyBucket.Tests;
using Shouldly;
using Xunit;
using System.Linq;

namespace PolyBucket.Tests.Features.Authentication.TwoFactorAuth
{
    [Collection("TestCollection")]
    public class EnableTwoFactorAuthTests : BaseIntegrationTest
    {
        private readonly PolyBucketDbContext _context;
        private readonly IEnableTwoFactorAuthRepository _repository;

        public EnableTwoFactorAuthTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _context = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            _repository = ServiceScope.ServiceProvider.GetRequiredService<IEnableTwoFactorAuthRepository>();
        }

        [Fact(DisplayName = "When enabling two-factor auth with a valid token, the enable two-factor auth controller enables it successfully and returns backup codes.")]
        public async Task Enable_WithValidToken_ShouldEnableSuccessfully()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create initialized 2FA
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 1
            };
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();

            var command = new EnableTwoFactorAuthCommand 
            { 
                UserId = user.Id, 
                Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
            };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<EnableTwoFactorAuthResponse>();
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("enabled successfully");
            result.BackupCodes.ShouldNotBeNull();
            result.BackupCodes.Count().ShouldBe(10); // Default backup code count

            _context.ChangeTracker.Clear();
            var updatedTwoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            updatedTwoFactorAuth.ShouldNotBeNull();
            updatedTwoFactorAuth.IsEnabled.ShouldBeTrue();
            updatedTwoFactorAuth.EnabledAt.ShouldNotBeNull();
            updatedTwoFactorAuth.Version.ShouldBe(2); // Version should be incremented

            // Verify backup codes are created
            var backupCodes = await _context.BackupCodes.Where(bc => bc.TwoFactorAuthId == twoFactorAuth.Id).ToListAsync();
            backupCodes.Count.ShouldBe(10);
            backupCodes.All(bc => !bc.IsUsed).ShouldBeTrue();
        }

        [Fact(DisplayName = "When enabling two-factor auth with an invalid TOTP token, the enable two-factor auth controller returns BadRequest.")]
        public async Task Enable_WithInvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create initialized 2FA
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 1
            };
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();

            var command = new EnableTwoFactorAuthCommand 
            { 
                UserId = user.Id, 
                Token = "000000" // Invalid token
            };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.ShouldContain("Invalid token");
        }

        [Fact(DisplayName = "When enabling two-factor auth that does not exist for the user, the enable two-factor auth controller returns BadRequest.")]
        public async Task Enable_WithNonExistentTwoFactorAuth_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            var command = new EnableTwoFactorAuthCommand 
            { 
                UserId = user.Id, 
                Token = "123456"
            };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.ShouldContain("initialized");
        }

        [Fact(DisplayName = "When enabling two-factor auth that is already enabled, the enable two-factor auth controller returns BadRequest.")]
        public async Task Enable_WithAlreadyEnabledTwoFactorAuth_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create already enabled 2FA
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
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
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();

            var command = new EnableTwoFactorAuthCommand 
            { 
                UserId = user.Id, 
                Token = "123456"
            };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.ShouldContain("2FA is already enabled");
        }

        [Fact(DisplayName = "When enabling two-factor auth without authentication, the enable two-factor auth controller returns Unauthorized.")]
        public async Task Enable_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new EnableTwoFactorAuthCommand 
            { 
                UserId = Guid.NewGuid(), 
                Token = "123456"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "When enabling two-factor auth for a user that does not match the authenticated token, the enable two-factor auth controller returns Unauthorized.")]
        public async Task Enable_WithDifferentUserInToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var user1 = await CreateTestUser("user1@test.com");
            var user2 = await CreateTestUser("user2@test.com");
            var token = await GetAuthToken(user1.Email, "TestPassword123!");
            var command = new EnableTwoFactorAuthCommand 
            { 
                UserId = user2.Id, 
                Token = "123456"
            };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "When enabling two-factor auth, the enable two-factor auth controller increments the version of the record.")]
        public async Task Enable_ShouldIncrementVersion()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create initialized 2FA
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 1
            };
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();

            var command = new EnableTwoFactorAuthCommand 
            { 
                UserId = user.Id, 
                Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
            };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            _context.ChangeTracker.Clear();
            var updatedTwoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            updatedTwoFactorAuth.ShouldNotBeNull();
            updatedTwoFactorAuth.Version.ShouldBe(2);
        }
    }
} 