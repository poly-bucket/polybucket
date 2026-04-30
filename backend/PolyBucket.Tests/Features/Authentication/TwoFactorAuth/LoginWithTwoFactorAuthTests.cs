using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Login.Repository;
using PolyBucket.Tests;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.TwoFactorAuth
{
    [Collection("TestCollection")]
    public class LoginWithTwoFactorAuthTests : BaseIntegrationTest
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILoginTwoFactorAuthService _loginTwoFactorAuthService;
        private readonly ILoginTwoFactorAuthRepository _loginTwoFactorAuthRepository;

        public LoginWithTwoFactorAuthTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _context = ServiceScope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            _loginTwoFactorAuthService = ServiceScope.ServiceProvider.GetRequiredService<ILoginTwoFactorAuthService>();
            _loginTwoFactorAuthRepository = ServiceScope.ServiceProvider.GetRequiredService<ILoginTwoFactorAuthRepository>();
        }

        [Fact(DisplayName = "When logging in with valid credentials and a valid two-factor token, the login controller returns a successful response.")]
        public async Task Login_WithValidCredentialsAndValidToken_ShouldReturnSuccess()
        {
            // Arrange
            var user = await UserFactory.CreateTestUser();
            var twoFactorAuth = await CreateTwoFactorAuth(user.Id, true);
            
            var loginCommand = new LoginCommand
            {
                EmailOrUsername = user.Email,
                Password = "TestPassword123!",
                TwoFactorToken = TotpTestHelper.GenerateCurrentTotp("JBSWY3DPEHPK3PXP")
            };

            // Act
            var client = Factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
            result.ShouldNotBeNull();
            result.Token.ShouldNotBeNullOrEmpty();
            result.RefreshToken.ShouldNotBeNullOrEmpty();
            result.RequiresTwoFactor.ShouldBeFalse();
        }

        [Fact(DisplayName = "When logging in with valid credentials and an invalid two-factor token, the login controller returns Unauthorized.")]
        public async Task Login_WithValidCredentialsAndInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = await UserFactory.CreateTestUser();
            var twoFactorAuth = await CreateTwoFactorAuth(user.Id, true);
            
            var loginCommand = new LoginCommand
            {
                EmailOrUsername = user.Email,
                Password = "TestPassword123!",
                TwoFactorToken = "000000" // Invalid token
            };

            // Act
            var client = Factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "When logging in with valid credentials and a valid backup code, the login controller returns a successful response and marks the backup code as used.")]
        public async Task Login_WithValidCredentialsAndValidBackupCode_ShouldReturnSuccess()
        {
            // Arrange
            var user = await UserFactory.CreateTestUser();
            var twoFactorAuth = await CreateTwoFactorAuth(user.Id, true);
            
            // Add a backup code
            var backupCode = new BackupCode
            {
                Id = Guid.NewGuid(),
                TwoFactorAuthId = twoFactorAuth.Id,
                Code = "12345678",
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedById = user.Id,
                Version = 1
            };
            await _context.BackupCodes.AddAsync(backupCode);
            await _context.SaveChangesAsync();
            
            var loginCommand = new LoginCommand
            {
                EmailOrUsername = user.Email,
                Password = "TestPassword123!",
                BackupCode = "12345678"
            };

            // Act
            var client = Factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
            result.ShouldNotBeNull();
            result.Token.ShouldNotBeNullOrEmpty();
            result.RefreshToken.ShouldNotBeNullOrEmpty();
            result.RequiresTwoFactor.ShouldBeFalse();

            _context.ChangeTracker.Clear();
            var updatedBackupCode = await _context.BackupCodes.FirstOrDefaultAsync(bc => bc.Id == backupCode.Id);
            updatedBackupCode.ShouldNotBeNull();
            updatedBackupCode.IsUsed.ShouldBeTrue();
            updatedBackupCode.UsedAt.ShouldNotBeNull();
        }

        [Fact(DisplayName = "When logging in with valid credentials and a backup code that has already been used, the login controller returns Unauthorized.")]
        public async Task Login_WithValidCredentialsAndUsedBackupCode_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = await UserFactory.CreateTestUser();
            var twoFactorAuth = await CreateTwoFactorAuth(user.Id, true);
            
            // Add a used backup code
            var backupCode = new BackupCode
            {
                Id = Guid.NewGuid(),
                TwoFactorAuthId = twoFactorAuth.Id,
                Code = "12345678",
                IsUsed = true,
                UsedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = user.Id,
                Version = 1
            };
            await _context.BackupCodes.AddAsync(backupCode);
            await _context.SaveChangesAsync();
            
            var loginCommand = new LoginCommand
            {
                EmailOrUsername = user.Email,
                Password = "TestPassword123!",
                BackupCode = "12345678"
            };

            // Act
            var client = Factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "When logging in with valid credentials but no two-factor token while two-factor auth is enabled, the login controller returns a response indicating that two-factor auth is required.")]
        public async Task Login_WithValidCredentialsAndNoTwoFactorToken_ShouldReturnRequiresTwoFactor()
        {
            // Arrange
            var user = await UserFactory.CreateTestUser();
            var twoFactorAuth = await CreateTwoFactorAuth(user.Id, true);
            
            var loginCommand = new LoginCommand
            {
                EmailOrUsername = user.Email,
                Password = "TestPassword123!"
                // No TwoFactorToken provided
            };

            // Act
            var client = Factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
            result.ShouldNotBeNull();
            result.RequiresTwoFactor.ShouldBeTrue();
            result.Token.ShouldBeNullOrEmpty();
            result.RefreshToken.ShouldBeNullOrEmpty();
        }

        [Fact(DisplayName = "When logging in with valid credentials while two-factor auth is disabled, the login controller returns a successful response.")]
        public async Task Login_WithValidCredentialsAndDisabledTwoFactorAuth_ShouldReturnSuccess()
        {
            // Arrange
            var user = await UserFactory.CreateTestUser();
            var twoFactorAuth = await CreateTwoFactorAuth(user.Id, false); // Disabled
            
            var loginCommand = new LoginCommand
            {
                EmailOrUsername = user.Email,
                Password = "TestPassword123!"
            };

            // Act
            var client = Factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
            result.ShouldNotBeNull();
            result.Token.ShouldNotBeNullOrEmpty();
            result.RefreshToken.ShouldNotBeNullOrEmpty();
            result.RequiresTwoFactor.ShouldBeFalse();
        }

        [Fact(DisplayName = "When logging in with valid credentials and no two-factor auth configured, the login controller returns a successful response.")]
        public async Task Login_WithValidCredentialsAndNoTwoFactorAuth_ShouldReturnSuccess()
        {
            // Arrange
            var user = await UserFactory.CreateTestUser();
            // No 2FA created
            
            var loginCommand = new LoginCommand
            {
                EmailOrUsername = user.Email,
                Password = "TestPassword123!"
            };

            // Act
            var client = Factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", loginCommand);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
            result.ShouldNotBeNull();
            result.Token.ShouldNotBeNullOrEmpty();
            result.RefreshToken.ShouldNotBeNullOrEmpty();
            result.RequiresTwoFactor.ShouldBeFalse();
        }

        private async Task<PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth> CreateTwoFactorAuth(Guid userId, bool isEnabled)
        {
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = isEnabled,
                EnabledAt = isEnabled ? DateTime.UtcNow.AddDays(-1) : null,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = userId,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedById = userId,
                Version = 1
            };
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();
            return twoFactorAuth;
        }
    }
} 