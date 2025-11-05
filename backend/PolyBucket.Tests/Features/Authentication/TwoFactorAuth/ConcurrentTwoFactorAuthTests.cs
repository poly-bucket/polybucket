using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain;
using PolyBucket.Tests;
using Shouldly;
using Xunit;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Tests.Features.Authentication.TwoFactorAuth
{
    public class ConcurrentTwoFactorAuthTests : BaseIntegrationTest
    {
        private readonly PolyBucketDbContext _context;

        public ConcurrentTwoFactorAuthTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _context = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        }

        [Fact]
        public async Task Enable_ConcurrentEnableAttempts_ShouldHandleConcurrencyGracefully()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");

            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
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
                Token = "123456"
            };

            // Act - Simulate concurrent requests
            SetAuthHeaders(token);
            
            var tasks = new[]
            {
                Client.PostAsJsonAsync("/api/auth/2fa/enable", command),
                Client.PostAsJsonAsync("/api/auth/2fa/enable", command),
                Client.PostAsJsonAsync("/api/auth/2fa/enable", command)
            };

            var responses = await Task.WhenAll(tasks);

            // Assert - One should succeed, others should fail with concurrency error
            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

            successCount.ShouldBe(1);
            conflictCount.ShouldBe(2);

            // Verify only one enablement succeeded
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.IsEnabled.ShouldBeTrue();
            finalTwoFactorAuth.Version.ShouldBeGreaterThan(1);
        }

        [Fact]
        public async Task Enable_ConcurrentEnableAndDisable_ShouldHandleConcurrencyGracefully()
        {
            // Arrange
            var user = await CreateTestUser();
            Console.WriteLine("User ID: {0}", user.Id);
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            Console.WriteLine("Token: {0}", token);

            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
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

            var enableCommand = new EnableTwoFactorAuthCommand
            {
                UserId = user.Id,
                Token = "123456"
            };

            var disableCommand = new DisableTwoFactorAuthCommand
            {
                UserId = user.Id
            };

            // Act - Simulate concurrent enable and disable
            SetAuthHeaders(token);
            
            var enableTask = Client.PostAsJsonAsync("/api/auth/2fa/enable", enableCommand);
            var disableTask = Client.PostAsJsonAsync("/api/auth/2fa/disable", disableCommand);

            var responses = await Task.WhenAll(enableTask, disableTask);

            // Assert - One should succeed, one should fail
            var enableResponse = responses[0];
            var disableResponse = responses[1];

            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            successCount.ShouldBe(1);

            // Verify final state
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            
            // Version should be incremented
            finalTwoFactorAuth.Version.ShouldBeGreaterThan(1);
        }

        [Fact]
        public async Task Disable_ConcurrentDisableAttempts_ShouldHandleConcurrencyGracefully()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");

            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                EnabledAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 2
            };
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();

            var command = new DisableTwoFactorAuthCommand
            {
                UserId = user.Id
            };

            // Act - Simulate concurrent disable requests
            SetAuthHeaders(token);
            
            var tasks = new[]
            {
                Client.PostAsJsonAsync("/api/auth/2fa/disable", command),
                Client.PostAsJsonAsync("/api/auth/2fa/disable", command)
            };

            var responses = await Task.WhenAll(tasks);

            // Assert - One should succeed, one should fail with concurrency error
            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

            successCount.ShouldBe(1);
            conflictCount.ShouldBe(1);

            // Verify only one disable succeeded
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.IsEnabled.ShouldBeFalse();
        }

        [Fact]
        public async Task RegenerateBackupCodes_ConcurrentRegenerateAttempts_ShouldHandleConcurrencyGracefully()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");

            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                EnabledAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 2
            };
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();

            var command = new RegenerateBackupCodesCommand
            {
                UserId = user.Id
            };

            // Act - Simulate concurrent regenerate requests
            SetAuthHeaders(token);
            
            var tasks = new[]
            {
                Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command),
                Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command),
                Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command)
            };

            var responses = await Task.WhenAll(tasks);

            // Assert - One should succeed, others should fail with concurrency error
            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

            successCount.ShouldBe(1);
            conflictCount.ShouldBe(2);

            // Verify backup codes were regenerated once
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.BackupCodes.Count.ShouldBe(10);
            finalTwoFactorAuth.Version.ShouldBeGreaterThan(2);
        }

        [Fact]
        public async Task Enable_WithVersionChange_ShouldIncrementVersion()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");

            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
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

            var initialVersion = twoFactorAuth.Version;

            var command = new EnableTwoFactorAuthCommand
            {
                UserId = user.Id,
                Token = "123456"
            };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            var updatedTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            updatedTwoFactorAuth.ShouldNotBeNull();
            updatedTwoFactorAuth.Version.ShouldBe(initialVersion + 1);
        }

        [Fact]
        public async Task Enable_ConcurrentEnableWithDirectDatabaseModification_ShouldDetectConcurrencyConflict()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");

            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
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
                Token = "123456"
            };

            // Act - Modify version directly in database to simulate concurrent modification
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE \"TwoFactorAuths\" SET \"Version\" = 2 WHERE \"Id\" = {0}", twoFactorAuth.Id);

            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert - Should fail with concurrency error
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var errorContent = await response.Content.ReadAsStringAsync();
            errorContent.ShouldContain("modified by another operation");
        }

        [Fact]
        public async Task Enable_ThenDisable_ThenEnableAgain_ShouldHandleSequentialOperations()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");

            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
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

            var enableCommand = new EnableTwoFactorAuthCommand
            {
                UserId = user.Id,
                Token = "123456"
            };

            var disableCommand = new DisableTwoFactorAuthCommand
            {
                UserId = user.Id
            };

            // Act - Sequential operations
            SetAuthHeaders(token);
            
            var enableResponse1 = await Client.PostAsJsonAsync("/api/auth/2fa/enable", enableCommand);
            enableResponse1.StatusCode.ShouldBe(HttpStatusCode.OK);

            var disableResponse = await Client.PostAsJsonAsync("/api/auth/2fa/disable", disableCommand);
            disableResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var enableResponse2 = await Client.PostAsJsonAsync("/api/auth/2fa/enable", enableCommand);
            enableResponse2.StatusCode.ShouldBe(HttpStatusCode.OK);

            // Assert - Final state should be enabled
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.IsEnabled.ShouldBeTrue();
            finalTwoFactorAuth.Version.ShouldBeGreaterThan(3);
        }

        [Fact]
        public async Task MultipleOperations_ShouldMaintainVersionConsistency()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");

            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
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

            var enableCommand = new EnableTwoFactorAuthCommand
            {
                UserId = user.Id,
                Token = "123456"
            };

            var regenerateCommand = new RegenerateBackupCodesCommand
            {
                UserId = user.Id
            };

            // Act - Sequential operations
            SetAuthHeaders(token);
            
            var enableResponse = await Client.PostAsJsonAsync("/api/auth/2fa/enable", enableCommand);
            enableResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var initialVersion = (await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id))?.Version ?? 0;

            var regenerateResponse = await Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", regenerateCommand);
            regenerateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            // Assert - Version should be incremented for each operation
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.Version.ShouldBe(initialVersion + 1);
        }
    }
}

