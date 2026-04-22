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
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Repository;
using PolyBucket.Tests;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.TwoFactorAuth
{
    [Collection("TestCollection")]
    public class DisableTwoFactorAuthTests : BaseIntegrationTest
    {
        private readonly PolyBucketDbContext _context;
        private readonly IDisableTwoFactorAuthRepository _repository;

        public DisableTwoFactorAuthTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _context = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            _repository = ServiceScope.ServiceProvider.GetRequiredService<IDisableTwoFactorAuthRepository>();
        }

        [Fact]
        public async Task Disable_WithEnabledTwoFactorAuth_ShouldDisableSuccessfully()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create enabled 2FA with backup codes
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

            // Add some backup codes
            var backupCodes = new[]
            {
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "12345678", IsUsed = false, CreatedAt = DateTime.UtcNow, CreatedById = user.Id, UpdatedAt = DateTime.UtcNow, UpdatedById = user.Id, Version = 1 },
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "87654321", IsUsed = false, CreatedAt = DateTime.UtcNow, CreatedById = user.Id, UpdatedAt = DateTime.UtcNow, UpdatedById = user.Id, Version = 1 }
            };
            await _context.BackupCodes.AddRangeAsync(backupCodes);
            await _context.SaveChangesAsync();

            var command = new DisableTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<DisableTwoFactorAuthResponse>();
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("disabled successfully");

            // Verify 2FA is disabled in database
            var updatedTwoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            updatedTwoFactorAuth.ShouldNotBeNull();
            updatedTwoFactorAuth.IsEnabled.ShouldBeFalse();
            updatedTwoFactorAuth.EnabledAt.ShouldBeNull();
            updatedTwoFactorAuth.Version.ShouldBe(2); // Version should be incremented

            // Verify all backup codes are marked as used
            var updatedBackupCodes = await _context.BackupCodes.Where(bc => bc.TwoFactorAuthId == twoFactorAuth.Id).ToListAsync();
            updatedBackupCodes.All(bc => bc.IsUsed).ShouldBeTrue();
            updatedBackupCodes.All(bc => bc.UsedAt.HasValue).ShouldBeTrue();
        }

        [Fact]
        public async Task Disable_WithNonExistentTwoFactorAuth_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            var command = new DisableTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.ShouldContain("2FA not found");
        }

        [Fact]
        public async Task Disable_WithAlreadyDisabledTwoFactorAuth_ShouldReturnBadRequest()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create disabled 2FA
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

            var command = new DisableTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.ShouldContain("2FA is not enabled");
        }

        [Fact]
        public async Task Disable_WithConcurrentRequests_ShouldHandleVersionConflicts()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create enabled 2FA
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

            var command = new DisableTwoFactorAuthCommand { UserId = user.Id };

            // Act - First request should succeed
            SetAuthHeaders(token);
            var response1 = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);
            response1.StatusCode.ShouldBe(HttpStatusCode.OK);

            // Second request should fail due to version conflict
            var response2 = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);
            response2.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Disable_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new DisableTwoFactorAuthCommand { UserId = Guid.NewGuid() };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Disable_WithDifferentUserInToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var user1 = await CreateTestUser("user1@test.com");
            var user2 = await CreateTestUser("user2@test.com");
            var token = await GetAuthToken(user1.Email, "TestPassword123!");
            var command = new DisableTwoFactorAuthCommand { UserId = user2.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Disable_ShouldMarkAllBackupCodesAsUsed()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create enabled 2FA with backup codes
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

            // Add backup codes with mixed usage status
            var backupCodes = new[]
            {
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "12345678", IsUsed = false, CreatedAt = DateTime.UtcNow, CreatedById = user.Id, UpdatedAt = DateTime.UtcNow, UpdatedById = user.Id, Version = 1 },
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "87654321", IsUsed = true, UsedAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow, CreatedById = user.Id, UpdatedAt = DateTime.UtcNow, UpdatedById = user.Id, Version = 1 },
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "11111111", IsUsed = false, CreatedAt = DateTime.UtcNow, CreatedById = user.Id, UpdatedAt = DateTime.UtcNow, UpdatedById = user.Id, Version = 1 }
            };
            await _context.BackupCodes.AddRangeAsync(backupCodes);
            await _context.SaveChangesAsync();

            var command = new DisableTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            // Verify all backup codes are marked as used
            var updatedBackupCodes = await _context.BackupCodes.Where(bc => bc.TwoFactorAuthId == twoFactorAuth.Id).ToListAsync();
            updatedBackupCodes.All(bc => bc.IsUsed).ShouldBeTrue();
            updatedBackupCodes.All(bc => bc.UsedAt.HasValue).ShouldBeTrue();
        }

        [Fact]
        public async Task Disable_ShouldIncrementVersion()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create enabled 2FA
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

            var command = new DisableTwoFactorAuthCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/disable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            // Verify version is incremented
            var updatedTwoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            updatedTwoFactorAuth.ShouldNotBeNull();
            updatedTwoFactorAuth.Version.ShouldBe(2);
        }
    }
} 