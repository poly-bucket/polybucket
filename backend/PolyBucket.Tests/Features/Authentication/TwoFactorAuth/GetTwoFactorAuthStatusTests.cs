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
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Repository;
using PolyBucket.Tests;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.TwoFactorAuth
{
    public class GetTwoFactorAuthStatusTests : BaseIntegrationTest
    {
        private readonly PolyBucketDbContext _context;
        private readonly IGetTwoFactorAuthStatusRepository _repository;

        public GetTwoFactorAuthStatusTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _context = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            _repository = ServiceScope.ServiceProvider.GetRequiredService<IGetTwoFactorAuthStatusRepository>();
        }

        [Fact]
        public async Task GetStatus_WithEnabledTwoFactorAuth_ShouldReturnEnabledStatus()
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

            // Act
            SetAuthHeaders(token);
            var response = await Client.GetAsync($"/api/auth/2fa/status");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTwoFactorAuthStatusResponse>();
            result.ShouldNotBeNull();
            result.IsEnabled.ShouldBeTrue();
            result.IsInitialized.ShouldBeTrue();
            result.EnabledAt.ShouldNotBeNull();
            result.RemainingBackupCodes.ShouldBe(0); // No backup codes created yet
        }

        [Fact]
        public async Task GetStatus_WithDisabledTwoFactorAuth_ShouldReturnDisabledStatus()
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

            // Act
            SetAuthHeaders(token);
            var response = await Client.GetAsync($"/api/auth/2fa/status");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTwoFactorAuthStatusResponse>();
            result.ShouldNotBeNull();
            result.IsEnabled.ShouldBeFalse();
            result.IsInitialized.ShouldBeTrue();
            result.EnabledAt.ShouldBeNull();
            result.RemainingBackupCodes.ShouldBe(0);
        }

        [Fact]
        public async Task GetStatus_WithNoTwoFactorAuth_ShouldReturnNotInitializedStatus()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");

            // Act
            SetAuthHeaders(token);
            var response = await Client.GetAsync($"/api/auth/2fa/status");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTwoFactorAuthStatusResponse>();
            result.ShouldNotBeNull();
            result.IsEnabled.ShouldBeFalse();
            result.IsInitialized.ShouldBeFalse();
            result.EnabledAt.ShouldBeNull();
            result.RemainingBackupCodes.ShouldBe(0);
        }

        [Fact]
        public async Task GetStatus_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Act
            var response = await Client.GetAsync($"/api/auth/2fa/status");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetStatus_WithAllUsedBackupCodes_ShouldReturnZeroRemaining()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create enabled 2FA with all used backup codes
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

            // Add all used backup codes
            var backupCodes = new[]
            {
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "12345678", IsUsed = true, UsedAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedById = user.Id, UpdatedAt = DateTime.UtcNow.AddDays(-1), UpdatedById = user.Id, Version = 1 },
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "87654321", IsUsed = true, UsedAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedById = user.Id, UpdatedAt = DateTime.UtcNow.AddDays(-1), UpdatedById = user.Id, Version = 1 }
            };
            await _context.BackupCodes.AddRangeAsync(backupCodes);
            await _context.SaveChangesAsync();

            // Act
            SetAuthHeaders(token);
            var response = await Client.GetAsync($"/api/auth/2fa/status");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTwoFactorAuthStatusResponse>();
            result.ShouldNotBeNull();
            result.IsEnabled.ShouldBeTrue();
            result.IsInitialized.ShouldBeTrue();
            result.RemainingBackupCodes.ShouldBe(0);
        }

        [Fact]
        public async Task GetStatus_WithNoBackupCodes_ShouldReturnZeroRemaining()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create enabled 2FA without backup codes
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

            // Act
            SetAuthHeaders(token);
            var response = await Client.GetAsync($"/api/auth/2fa/status");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTwoFactorAuthStatusResponse>();
            result.ShouldNotBeNull();
            result.IsEnabled.ShouldBeTrue();
            result.IsInitialized.ShouldBeTrue();
            result.RemainingBackupCodes.ShouldBe(0);
        }

        [Fact]
        public async Task GetStatus_WithMixedBackupCodes_ShouldReturnCorrectCount()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            
            // Create enabled 2FA with mixed backup codes
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

            // Add mixed backup codes
            var backupCodes = new[]
            {
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "12345678", IsUsed = true, UsedAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedById = user.Id, UpdatedAt = DateTime.UtcNow.AddDays(-1), UpdatedById = user.Id, Version = 1 },
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "87654321", IsUsed = false, CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedById = user.Id, UpdatedAt = DateTime.UtcNow.AddDays(-1), UpdatedById = user.Id, Version = 1 },
                new BackupCode { Id = Guid.NewGuid(), TwoFactorAuthId = twoFactorAuth.Id, Code = "11111111", IsUsed = false, CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedById = user.Id, UpdatedAt = DateTime.UtcNow.AddDays(-1), UpdatedById = user.Id, Version = 1 }
            };
            await _context.BackupCodes.AddRangeAsync(backupCodes);
            await _context.SaveChangesAsync();

            // Act
            SetAuthHeaders(token);
            var response = await Client.GetAsync($"/api/auth/2fa/status");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTwoFactorAuthStatusResponse>();
            result.ShouldNotBeNull();
            result.IsEnabled.ShouldBeTrue();
            result.IsInitialized.ShouldBeTrue();
            result.RemainingBackupCodes.ShouldBe(2); // Two unused backup codes
        }

        [Fact]
        public async Task GetStatus_ShouldIncludeCorrectTimestamps()
        {
            // Arrange
            var user = await CreateTestUser();
            var token = await GetAuthToken(user.Email, "TestPassword123!");
            var enabledAt = DateTime.UtcNow.AddDays(-5);
            var lastUsedAt = DateTime.UtcNow.AddHours(-3);
            
            // Create enabled 2FA with specific timestamps
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                EnabledAt = enabledAt,
                LastUsedAt = lastUsedAt,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 1
            };
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();

            // Act
            SetAuthHeaders(token);
            var response = await Client.GetAsync($"/api/auth/2fa/status");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTwoFactorAuthStatusResponse>();
            result.ShouldNotBeNull();
            result.EnabledAt.ShouldBe(enabledAt);
            result.LastUsedAt.ShouldBe(lastUsedAt);
        }

        [Fact]
        public async Task GetStatus_WithRepositoryMethod_ShouldReturnCorrectData()
        {
            // Arrange
            var user = await CreateTestUser();
            
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

            // Act
            var result = await _repository.GetByUserIdAsync(user.Id);

            // Assert
            result.ShouldNotBeNull();
            result.UserId.ShouldBe(user.Id);
            result.IsEnabled.ShouldBeTrue();
            result.EnabledAt.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetStatus_WithNonExistentUser_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByUserIdAsync(Guid.NewGuid());

            // Assert
            result.ShouldBeNull();
        }
    }
} 