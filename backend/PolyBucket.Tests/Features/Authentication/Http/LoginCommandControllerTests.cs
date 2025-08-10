using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Login.Http;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using PolyBucket.Api.Features.ACL.Domain;
using RefreshTokenModel = PolyBucket.Api.Features.Authentication.Domain.RefreshToken;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Authentication.Login.Repository;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class LoginCommandControllerTests : BaseIntegrationTest
    {
        private readonly IAuthenticationRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILoginTwoFactorAuthService _loginTwoFactorAuthService;
        private readonly ILoginTwoFactorAuthRepository _loginTwoFactorAuthRepository;

        public LoginCommandControllerTests(TestCollectionFixture testFixture) : base(testFixture)
        {
            _authRepository = ServiceScope.ServiceProvider.GetRequiredService<IAuthenticationRepository>();
            _tokenService = ServiceScope.ServiceProvider.GetRequiredService<ITokenService>();
            _passwordHasher = ServiceScope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            _loginTwoFactorAuthService = ServiceScope.ServiceProvider.GetRequiredService<ILoginTwoFactorAuthService>();
            _loginTwoFactorAuthRepository = ServiceScope.ServiceProvider.GetRequiredService<ILoginTwoFactorAuthRepository>();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var user = await CreateTestUser();
            var command = new LoginCommand
            {
                EmailOrUsername = user.Email,
                Password = "TestPassword123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/login", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginCommandResponse>();
            result.ShouldNotBeNull();
            result.Token.ShouldNotBeNullOrEmpty();
            result.RefreshToken.ShouldNotBeNullOrEmpty();
            result.RequiresPasswordChange.ShouldBeFalse();
            result.RequiresFirstTimeSetup.ShouldBeFalse();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new LoginCommand
            {
                EmailOrUsername = "nonexistent@test.com",
                Password = "wrongpassword"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/login", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithEmptyCredentials_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new LoginCommand
            {
                EmailOrUsername = "",
                Password = ""
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/login", command);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }
} 