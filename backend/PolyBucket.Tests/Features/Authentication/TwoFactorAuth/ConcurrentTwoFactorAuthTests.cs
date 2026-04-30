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
    [Collection("TestCollection")]
    public class ConcurrentTwoFactorAuthTests : BaseIntegrationTest
    {
        private readonly PolyBucketDbContext _context;

        public ConcurrentTwoFactorAuthTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _context = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        }

        [Fact(DisplayName = "When multiple concurrent enable two-factor auth requests are made, the system handles concurrency gracefully and allows only one to succeed.")]
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
                Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
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

            _context.ChangeTracker.Clear();
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.IsEnabled.ShouldBeTrue();
            finalTwoFactorAuth.Version.ShouldBeGreaterThan(1);
        }

        [Fact(DisplayName = "When concurrent enable and disable two-factor auth requests are made, the system handles concurrency gracefully.")]
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
                Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
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

            _context.ChangeTracker.Clear();
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            
            // Version should be incremented
            finalTwoFactorAuth.Version.ShouldBeGreaterThan(1);
        }

        [Fact(DisplayName = "When multiple concurrent disable two-factor auth requests are made, the system handles concurrency gracefully and allows only one to succeed.")]
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

            _context.ChangeTracker.Clear();
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.IsEnabled.ShouldBeFalse();
        }

        [Fact(DisplayName = "When multiple concurrent regenerate backup codes requests are made, the system handles concurrency gracefully and allows only one to succeed.")]
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

            await _context.BackupCodes.AddAsync(new BackupCode
            {
                Id = Guid.NewGuid(),
                TwoFactorAuthId = twoFactorAuth.Id,
                Code = "11111111",
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedById = user.Id,
                Version = 1
            });
            await _context.SaveChangesAsync();

            var command = new RegenerateBackupCodesCommand
            {
                UserId = user.Id
            };

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

            _context.ChangeTracker.Clear();
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.BackupCodes.Count.ShouldBe(10);
            finalTwoFactorAuth.Version.ShouldBeGreaterThan(2);
        }

        [Fact(DisplayName = "When enabling two-factor auth, the system increments the version of the two-factor auth record.")]
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
                Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
            };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            _context.ChangeTracker.Clear();
            var updatedTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            updatedTwoFactorAuth.ShouldNotBeNull();
            updatedTwoFactorAuth.Version.ShouldBe(initialVersion + 1);
        }

        [Fact(DisplayName = "When enabling two-factor auth after the database row version has been bumped directly, the handler loads current state and enables successfully.")]
        public async Task Enable_AfterDirectVersionBump_ShouldEnableSuccessfully()
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
                Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
            };

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE \"TwoFactorAuths\" SET \"Version\" = 2 WHERE \"Id\" = {0}", twoFactorAuth.Id);

            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/enable", command);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            _context.ChangeTracker.Clear();
            var updated = await _context.TwoFactorAuths.AsNoTracking().FirstAsync(tfa => tfa.UserId == user.Id);
            updated.IsEnabled.ShouldBeTrue();
            updated.Version.ShouldBe(3);
        }

        [Fact(DisplayName = "When two-factor auth is enabled, then disabled, then enabled again, the system handles each sequential operation successfully.")]
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
                Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
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

            enableCommand.Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP");
            var enableResponse2 = await Client.PostAsJsonAsync("/api/auth/2fa/enable", enableCommand);
            enableResponse2.StatusCode.ShouldBe(HttpStatusCode.OK);

            _context.ChangeTracker.Clear();
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.IsEnabled.ShouldBeTrue();
            finalTwoFactorAuth.Version.ShouldBeGreaterThan(3);
        }

        [Fact(DisplayName = "When performing multiple two-factor auth operations, the system maintains version consistency across each change.")]
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
                Token = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
            };

            var regenerateCommand = new RegenerateBackupCodesCommand
            {
                UserId = user.Id
            };

            // Act - Sequential operations
            SetAuthHeaders(token);
            
            var enableResponse = await Client.PostAsJsonAsync("/api/auth/2fa/enable", enableCommand);
            enableResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            _context.ChangeTracker.Clear();
            var initialVersion = (await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id))?.Version ?? 0;

            var regenerateResponse = await Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", regenerateCommand);
            regenerateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            _context.ChangeTracker.Clear();
            var finalTwoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            finalTwoFactorAuth.ShouldNotBeNull();
            finalTwoFactorAuth.Version.ShouldBe(initialVersion + 1);
        }
    }
}

