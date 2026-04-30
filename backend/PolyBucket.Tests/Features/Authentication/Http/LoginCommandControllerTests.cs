using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Login.Repository;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    [Collection("TestCollection")]
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

        [Fact(DisplayName = "When logging in with valid credentials, the login controller returns a successful response.")]
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

        [Fact(DisplayName = "When logging in with invalid credentials, the login controller returns Unauthorized.")]
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

        [Fact(DisplayName = "When logging in with empty credentials, the login controller returns BadRequest.")]
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