using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Register.Domain;
using PolyBucket.Api.Features.Authentication.Register.Http;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Users.Domain;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Domain;
using Microsoft.EntityFrameworkCore;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class RegisterCommandControllerTests : IDisposable
    {
        private readonly Mock<IAuthenticationRepository> _authRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IEmailSettingsResolver> _emailSettingsResolverMock;
        private readonly Mock<IAccountEmailService> _accountEmailServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
        private readonly PolyBucketDbContext _dbContext;
        private readonly RegisterController _controller;
        private readonly RegisterCommandHandler _handler;

        public RegisterCommandControllerTests()
        {
            _authRepositoryMock = new Mock<IAuthenticationRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _emailSettingsResolverMock = new Mock<IEmailSettingsResolver>();
            _accountEmailServiceMock = new Mock<IAccountEmailService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();
            var dbOptions = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new PolyBucketDbContext(dbOptions);
            _dbContext.Roles.Add(new Role
            {
                Id = Guid.NewGuid(),
                Name = "User",
                Description = "Standard user role",
                IsActive = true,
                IsDefault = true,
                IsSystemRole = true,
                CanBeDeleted = false,
                Priority = 100
            });
            _dbContext.SaveChanges();

            _emailSettingsResolverMock
                .Setup(x => x.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EffectiveEmailSettings());

            _handler = new RegisterCommandHandler(
                _authRepositoryMock.Object,
                _tokenServiceMock.Object,
                _emailSettingsResolverMock.Object,
                _accountEmailServiceMock.Object,
                _passwordHasherMock.Object,
                _loggerMock.Object,
                _dbContext);

            _controller = new RegisterController(_handler, Mock.Of<ILogger<RegisterController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        private void SetupEmailSettings(bool requireVerification)
        {
            _emailSettingsResolverMock
                .Setup(x => x.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EffectiveEmailSettings
                {
                    Transport = EmailTransportKind.Log,
                    FromAddress = "noreply@example.com",
                    PublicBaseUrl = "http://localhost:3000",
                    RequireEmailVerification = requireVerification
                });
        }

        [Fact(DisplayName = "When registering with a valid command, the register controller returns Ok with authentication data.")]
        public async Task Register_ValidCommand_ShouldReturnOkWithAuthentication()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Country = "US",
                UserAgent = "Test User Agent"
            };

            var authResponse = new AuthenticationResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                User = new UserInfo
                {
                    Id = Guid.NewGuid(),
                    Email = command.Email,
                    Username = command.Username,
                    Role = "User"
                }
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Returns(authResponse);
            SetupEmailSettings(requireVerification: false);
            _accountEmailServiceMock
                .Setup(x => x.SendWelcomeAsync(It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmailEnqueueOutcome.Queued);

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value.ShouldBeOfType<RegisterCommandResponse>();
            response.Authentication.ShouldNotBeNull();
            response.Authentication.AccessToken.ShouldBe("test-access-token");
            response.RequiresEmailVerification.ShouldBeFalse();

            // Verify all expected calls were made
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(command.Username), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once);
            _accountEmailServiceMock.Verify(x => x.SendWelcomeAsync(
                It.Is<User>(u => u.Email == command.Email && u.Username == command.Username),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When registering while email verification is enabled, the register controller returns Ok without exposing the verification token.")]
        public async Task Register_WithEmailVerificationEnabled_ShouldReturnOkWithoutVerificationToken()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            var authResponse = new AuthenticationResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                User = new UserInfo { Id = Guid.NewGuid() }
            };

            var verificationToken = "test-verification-token";

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _authRepositoryMock.Setup(x => x.CreateEmailVerificationTokenAsync(It.IsAny<EmailVerificationToken>()))
                .ReturnsAsync(It.IsAny<EmailVerificationToken>());
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Returns(authResponse);
            _tokenServiceMock.Setup(x => x.GenerateEmailVerificationToken())
                .Returns(verificationToken);
            SetupEmailSettings(requireVerification: true);
            _accountEmailServiceMock
                .Setup(x => x.SendVerificationAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmailEnqueueOutcome.Queued);

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value.ShouldBeOfType<RegisterCommandResponse>();
            response.RequiresEmailVerification.ShouldBeTrue();
            typeof(RegisterCommandResponse).GetProperty("EmailVerificationToken").ShouldBeNull();
            System.Text.Json.JsonSerializer.Serialize(response).ShouldNotContain(verificationToken);

            // Verify verification email was queued
            _accountEmailServiceMock.Verify(x => x.SendVerificationAsync(
                It.Is<User>(u => u.Email == command.Email),
                verificationToken,
                RegisterCommandHandler.EmailVerificationLifetime,
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateEmailVerificationTokenAsync(It.Is<EmailVerificationToken>(t =>
                t.Token == TokenHasher.Hash(verificationToken)
                && t.Token != verificationToken
                && t.Purpose == EmailVerificationPurpose.VerifyAddress)), Times.Once);
        }

        [Fact(DisplayName = "When registering, the register controller persists the issued refresh token so it can be refreshed later.")]
        public async Task Register_ValidCommand_ShouldPersistRefreshToken()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email)).ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username)).ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>())).Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Returns(new AuthenticationResponse
                {
                    AccessToken = "token",
                    RefreshToken = "refresh-token",
                    RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = new UserInfo { Id = Guid.NewGuid() }
                });

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            _authRepositoryMock.Verify(x => x.CreateRefreshTokenAsync(It.Is<RefreshToken>(t => t.Token == "refresh-token")), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.Is<User>(u => u.EmailVerifiedAt == null)), Times.Once);
        }

        [Fact(DisplayName = "When registering while email delivery is disabled, the register controller returns Ok and queues no email.")]
        public async Task Register_WithEmailDisabled_ShouldNotQueueEmail()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email)).ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username)).ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>())).Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Returns(new AuthenticationResponse { AccessToken = "token", User = new UserInfo { Id = Guid.NewGuid() } });

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            var response = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<RegisterCommandResponse>();
            response.RequiresEmailVerification.ShouldBeFalse();
            _accountEmailServiceMock.VerifyNoOtherCalls();
        }

        [Fact(DisplayName = "When registering with an email that is already taken, the register controller returns Conflict.")]
        public async Task Register_DuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "existing@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ConflictObjectResult>();
            var conflictResult = (ConflictObjectResult)result;
            conflictResult.Value.ShouldNotBeNull();

            // Verify only email check was made
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact(DisplayName = "When registering with a username that is already taken, the register controller returns Conflict.")]
        public async Task Register_DuplicateUsername_ShouldReturnConflict()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "existinguser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ConflictObjectResult>();
            var conflictResult = (ConflictObjectResult)result;
            conflictResult.Value.ShouldNotBeNull();

            // Verify both checks were made but no user created
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(command.Username), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact(DisplayName = "When registering with an invalid model state, the register controller returns BadRequest.")]
        public async Task Register_InvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "invalid-email",
                Username = "",
                Password = "short",
                ConfirmPassword = "different",
                UserAgent = "Test User Agent"
            };

            _controller.ModelState.AddModelError("Email", "Invalid email format");
            _controller.ModelState.AddModelError("Username", "Username is required");
            _controller.ModelState.AddModelError("Password", "Password too short");
            _controller.ModelState.AddModelError("ConfirmPassword", "Passwords do not match");

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.Value.ShouldNotBeNull();

            // Verify no repository calls were made
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact(DisplayName = "When registering and the repository throws an exception, the register controller returns InternalServerError.")]
        public async Task Register_RepositoryThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify only the first check was attempted
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact(DisplayName = "When registering and the token service throws an exception, the register controller returns InternalServerError.")]
        public async Task Register_TokenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Throws(new Exception("Token generation failed"));

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify user was created but token generation failed
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(command.Username), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Never);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(It.IsAny<User>()), Times.Once);
        }

        [Fact(DisplayName = "When registering and queueing the email throws an exception, the register controller returns InternalServerError.")]
        public async Task Register_EmailServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            var authResponse = new AuthenticationResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                User = new UserInfo { Id = Guid.NewGuid() }
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Returns(authResponse);
            SetupEmailSettings(requireVerification: false);
            _accountEmailServiceMock
                .Setup(x => x.SendWelcomeAsync(It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Email service failed"));

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify user was created and token generated but email failed
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(command.Username), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(It.IsAny<User>()), Times.Once);
            _accountEmailServiceMock.Verify(x => x.SendWelcomeAsync(It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
