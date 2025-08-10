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
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository;
using PolyBucket.Tests;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.TwoFactorAuth
{
    public class RegenerateBackupCodesTests : BaseIntegrationTest
    {
        private readonly PolyBucketDbContext _context;
        private readonly IRegenerateBackupCodesRepository _repository;

        public RegenerateBackupCodesTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _context = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            _repository = ServiceScope.ServiceProvider.GetRequiredService<IRegenerateBackupCodesRepository>();
        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();
        }

        [Fact]
        public async Task Regenerate_WithEnabledTwoFactorAuth_ShouldRegenerateSuccessfully()
        {
            // Arrange
            await InitializeAsync();
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create enabled 2FA with existing backup codes
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

            // Add some existing backup codes
            var existingBackupCodes = new[]
            {
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "12345678", IsUsed = false, CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedById = user.Id, UpdatedAt = DateTime.UtcNow.AddDays(-1), UpdatedById = user.Id, Version = 1 },
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "87654321", IsUsed = true, UsedAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedById = user.Id, UpdatedAt = DateTime.UtcNow.AddDays(-1), UpdatedById = user.Id, Version = 1 }
            };
            await _context.BackupCodes.AddRangeAsync(existingBackupCodes);
            await _context.SaveChangesAsync();

            var command = new RegenerateBackupCodesCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<RegenerateBackupCodesResponse>();
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            result.Message.ShouldContain("regenerated successfully");
            result.BackupCodes.ShouldNotBeNull();
            result.BackupCodes.Count().ShouldBe(10); // Default backup code count

            // Verify old backup codes are marked as used
            var oldBackupCodes = await _context.BackupCodes.Where(bc => bc.TwoFactorAuthId == twoFactorAuth.Id && !result.BackupCodes.Contains(bc.Code)).ToListAsync();
            oldBackupCodes.All(bc => bc.IsUsed).ShouldBeTrue();
            oldBackupCodes.All(bc => bc.UsedAt.HasValue).ShouldBeTrue();

            // Verify new backup codes are created
            var newBackupCodes = await _context.BackupCodes.Where(bc => result.BackupCodes.Contains(bc.Code)).ToListAsync();
            newBackupCodes.Count.ShouldBe(10);
            newBackupCodes.All(bc => !bc.IsUsed).ShouldBeTrue();
        }

        [Fact]
        public async Task Regenerate_WithDisabledTwoFactorAuth_ShouldReturnBadRequest()
        {
            // Arrange
            await InitializeAsync();
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

            var command = new RegenerateBackupCodesCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.ShouldContain("2FA is not enabled");
        }

        [Fact]
        public async Task Regenerate_WithNonExistentTwoFactorAuth_ShouldReturnBadRequest()
        {
            // Arrange
            await InitializeAsync();
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            var command = new RegenerateBackupCodesCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.ShouldContain("2FA not found");
        }

        [Fact]
        public async Task Regenerate_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            await InitializeAsync();
            var command = new RegenerateBackupCodesCommand { UserId = Guid.NewGuid() };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Regenerate_WithDifferentUserInToken_ShouldReturnUnauthorized()
        {
            // Arrange
            await InitializeAsync();
            var user1 = await CreateTestUser("user1@test.com");
            var user2 = await CreateTestUser("user2@test.com");
            var token = await GetAuthToken(user1.Email, "TestPassword123!");
            var command = new RegenerateBackupCodesCommand { UserId = user2.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Regenerate_ShouldIncrementVersion()
        {
            // Arrange
            await InitializeAsync();
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

            var command = new RegenerateBackupCodesCommand { UserId = user.Id };

            // Act
            SetAuthHeaders(token);
            var response = await Client.PostAsJsonAsync("/api/auth/2fa/regenerate-backup-codes", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            // Verify version is incremented
            var updatedTwoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(tfa => tfa.UserId == user.Id);
            updatedTwoFactorAuth.ShouldNotBeNull();
            updatedTwoFactorAuth.Version.ShouldBe(2);
        }
    }
} 