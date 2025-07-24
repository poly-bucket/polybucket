using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.OAuth.Domain;
using PolyBucket.Api.Features.Authentication.OAuth.Http;
using PolyBucket.Api.Features.Authentication.Services;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class OAuthControllerTests
    {
        private readonly Mock<IOAuthService> _oauthServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<OAuthController>> _loggerMock;
        private readonly OAuthController _controller;

        public OAuthControllerTests()
        {
            _oauthServiceMock = new Mock<IOAuthService>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<OAuthController>>();

            _controller = new OAuthController(
                _oauthServiceMock.Object,
                _configurationMock.Object,
                _loggerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task GetAuthorizationUrl_ValidProvider_ShouldReturnOkWithAuthUrl()
        {
            // Arrange
            var provider = "google";
            var redirectUri = "http://localhost:3000/auth/callback";
            var state = "test-state";
            var expectedAuthUrl = "https://accounts.google.com/oauth/authorize?client_id=test&redirect_uri=http://localhost:3000/auth/callback&state=test-state";

            _oauthServiceMock.Setup(x => x.GetAuthorizationUrlAsync(provider, redirectUri, state))
                .ReturnsAsync(expectedAuthUrl);

            // Act
            var result = await _controller.GetAuthorizationUrl(provider, redirectUri, state);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value;
            response.ShouldNotBeNull();

            // Verify the response structure
            var responseObject = response.GetType();
            var authUrlProperty = responseObject.GetProperty("authorizationUrl");
            var stateProperty = responseObject.GetProperty("state");
            
            authUrlProperty.ShouldNotBeNull();
            stateProperty.ShouldNotBeNull();
            
            authUrlProperty.GetValue(response).ShouldBe(expectedAuthUrl);
            stateProperty.GetValue(response).ShouldBe(state);

            // Verify service was called correctly
            _oauthServiceMock.Verify(x => x.GetAuthorizationUrlAsync(provider, redirectUri, state), Times.Once);
        }

        [Fact]
        public async Task GetAuthorizationUrl_WithoutState_ShouldReturnOkWithGeneratedState()
        {
            // Arrange
            var provider = "github";
            var redirectUri = "http://localhost:3000/auth/callback";
            var expectedAuthUrl = "https://github.com/login/oauth/authorize?client_id=test&redirect_uri=http://localhost:3000/auth/callback";

            _oauthServiceMock.Setup(x => x.GetAuthorizationUrlAsync(provider, redirectUri, It.IsAny<string>()))
                .ReturnsAsync(expectedAuthUrl);

            // Act
            var result = await _controller.GetAuthorizationUrl(provider, redirectUri, null);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value;
            response.ShouldNotBeNull();

            // Verify state was generated
            var responseObject = response.GetType();
            var stateProperty = responseObject.GetProperty("state");
            stateProperty.ShouldNotBeNull();
            var stateValue = stateProperty.GetValue(response) as string;
            stateValue.ShouldNotBeNullOrEmpty();

            // Verify service was called with generated state
            _oauthServiceMock.Verify(x => x.GetAuthorizationUrlAsync(provider, redirectUri, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAuthorizationUrl_InvalidProvider_ShouldReturnBadRequest()
        {
            // Arrange
            var provider = "invalid-provider";
            var redirectUri = "http://localhost:3000/auth/callback";
            var state = "test-state";

            _oauthServiceMock.Setup(x => x.GetAuthorizationUrlAsync(provider, redirectUri, state))
                .ThrowsAsync(new Exception("Invalid OAuth provider"));

            // Act
            var result = await _controller.GetAuthorizationUrl(provider, redirectUri, state);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.Value.ShouldNotBeNull();

            // Verify service was called
            _oauthServiceMock.Verify(x => x.GetAuthorizationUrlAsync(provider, redirectUri, state), Times.Once);
        }

        [Fact]
        public async Task OAuthCallback_ValidCode_ShouldReturnOkWithAuthentication()
        {
            // Arrange
            var provider = "google";
            var command = new OAuthCallbackCommand
            {
                Provider = provider,
                Code = "valid-auth-code",
                State = "test-state",
                UserAgent = "Test User Agent"
            };

            var authResponse = new AuthenticationResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                User = new UserInfo
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.com",
                    Username = "testuser",
                    Role = "User"
                }
            };

            _configurationMock.Setup(x => x["AppSettings:Frontend:BaseUrl"])
                .Returns("http://localhost:3000");
            _oauthServiceMock.Setup(x => x.AuthenticateWithOAuthAsync(
                provider, 
                command.Code, 
                "http://localhost:3000/auth/callback", 
                It.IsAny<string>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.OAuthCallback(provider, command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value.ShouldBeOfType<OAuthCallbackCommandResponse>();
            response.Authentication.ShouldNotBeNull();
            response.Authentication.AccessToken.ShouldBe("test-access-token");
            response.Provider.ShouldBe(provider);

            // Verify service was called correctly
            _oauthServiceMock.Verify(x => x.AuthenticateWithOAuthAsync(
                provider, 
                command.Code, 
                "http://localhost:3000/auth/callback", 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task OAuthCallback_ProviderMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var provider = "google";
            var command = new OAuthCallbackCommand
            {
                Provider = "github", // Different provider
                Code = "valid-auth-code",
                State = "test-state",
                UserAgent = "Test User Agent"
            };

            // Act
            var result = await _controller.OAuthCallback(provider, command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.Value.ShouldNotBeNull();

            // Verify service was not called
            _oauthServiceMock.Verify(x => x.AuthenticateWithOAuthAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task OAuthCallback_WithError_ShouldReturnBadRequest()
        {
            // Arrange
            var provider = "google";
            var command = new OAuthCallbackCommand
            {
                Provider = provider,
                Code = "",
                Error = "access_denied",
                ErrorDescription = "User denied access",
                UserAgent = "Test User Agent"
            };

            // Act
            var result = await _controller.OAuthCallback(provider, command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.Value.ShouldNotBeNull();

            // Verify error was included in response
            var errorResponse = badRequestResult.Value?.GetType().GetProperty("message");
            errorResponse.ShouldNotBeNull();
            errorResponse!.GetValue(badRequestResult.Value).ShouldBe("User denied access");

            // Verify service was not called
            _oauthServiceMock.Verify(x => x.AuthenticateWithOAuthAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task OAuthCallback_InvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var provider = "google";
            var command = new OAuthCallbackCommand
            {
                Provider = "", // Invalid empty provider
                Code = "", // Invalid empty code
                UserAgent = "Test User Agent"
            };

            _controller.ModelState.AddModelError("Provider", "Provider is required");
            _controller.ModelState.AddModelError("Code", "Code is required");

            // Act
            var result = await _controller.OAuthCallback(provider, command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.Value.ShouldNotBeNull();

            // Verify service was not called
            _oauthServiceMock.Verify(x => x.AuthenticateWithOAuthAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task OAuthCallback_AuthenticationFails_ShouldReturnUnauthorized()
        {
            // Arrange
            var provider = "google";
            var command = new OAuthCallbackCommand
            {
                Provider = provider,
                Code = "invalid-auth-code",
                State = "test-state",
                UserAgent = "Test User Agent"
            };

            _configurationMock.Setup(x => x["AppSettings:Frontend:BaseUrl"])
                .Returns("http://localhost:3000");
            _oauthServiceMock.Setup(x => x.AuthenticateWithOAuthAsync(
                provider, 
                command.Code, 
                "http://localhost:3000/auth/callback", 
                It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid authorization code"));

            // Act
            var result = await _controller.OAuthCallback(provider, command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.ShouldNotBeNull();

            // Verify service was called
            _oauthServiceMock.Verify(x => x.AuthenticateWithOAuthAsync(
                provider, 
                command.Code, 
                "http://localhost:3000/auth/callback", 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task OAuthCallback_ServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var provider = "google";
            var command = new OAuthCallbackCommand
            {
                Provider = provider,
                Code = "valid-auth-code",
                State = "test-state",
                UserAgent = "Test User Agent"
            };

            _configurationMock.Setup(x => x["AppSettings:Frontend:BaseUrl"])
                .Returns("http://localhost:3000");
            _oauthServiceMock.Setup(x => x.AuthenticateWithOAuthAsync(
                provider, 
                command.Code, 
                "http://localhost:3000/auth/callback", 
                It.IsAny<string>()))
                .ThrowsAsync(new Exception("OAuth service unavailable"));

            // Act
            var result = await _controller.OAuthCallback(provider, command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify service was called
            _oauthServiceMock.Verify(x => x.AuthenticateWithOAuthAsync(
                provider, 
                command.Code, 
                "http://localhost:3000/auth/callback", 
                It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData("google")]
        [InlineData("github")]
        [InlineData("microsoft")]
        public async Task GetAuthorizationUrl_SupportedProviders_ShouldReturnOk(string provider)
        {
            // Arrange
            var redirectUri = "http://localhost:3000/auth/callback";
            var state = "test-state";
            var expectedAuthUrl = $"https://{provider}.com/oauth/authorize";

            _oauthServiceMock.Setup(x => x.GetAuthorizationUrlAsync(provider, redirectUri, state))
                .ReturnsAsync(expectedAuthUrl);

            // Act
            var result = await _controller.GetAuthorizationUrl(provider, redirectUri, state);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();

            // Verify service was called
            _oauthServiceMock.Verify(x => x.GetAuthorizationUrlAsync(provider, redirectUri, state), Times.Once);
        }

        [Fact]
        public async Task OAuthCallback_EmptyErrorDescription_ShouldUseGenericMessage()
        {
            // Arrange
            var provider = "google";
            var command = new OAuthCallbackCommand
            {
                Provider = provider,
                Code = "",
                Error = "server_error",
                ErrorDescription = null, // No description provided
                UserAgent = "Test User Agent"
            };

            // Act
            var result = await _controller.OAuthCallback(provider, command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var errorResponse = badRequestResult.Value?.GetType().GetProperty("message");
            errorResponse!.GetValue(badRequestResult.Value).ShouldBe("OAuth authentication failed");
        }
    }
} 