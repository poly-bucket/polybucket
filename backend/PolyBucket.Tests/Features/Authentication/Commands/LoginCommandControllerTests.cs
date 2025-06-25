using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Commands;
using Shouldly;
using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Tests.Factories;
using System.Net;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Configuration;
using PolyBucket.Api.Features.Users.Repository;

namespace PolyBucket.Tests.Features.Authentication.Commands
{
    public class LoginCommandControllerTests : IDisposable
    {
        private readonly Mock<LoginCommandHandler> _mockHandler;
        private readonly Mock<ILogger<LoginCommandController>> _mockLogger;
        private readonly LoginCommandController _controller;
        private readonly PolyBucketDbContext _context;
        private readonly TestUserFactory _userFactory;

        public LoginCommandControllerTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: "TestAuthDatabase")
                .Options;
            _context = new PolyBucketDbContext(options);

            var mockRepo = new Mock<IUserRepository>();
            var mockConfig = new Mock<IConfiguration>();
            _mockHandler = new Mock<LoginCommandHandler>(mockRepo.Object, mockConfig.Object, null); 
            _mockLogger = new Mock<ILogger<LoginCommandController>>();
            _controller = new LoginCommandController(_mockHandler.Object, _mockLogger.Object);
            
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _userFactory = new TestUserFactory(_context);
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.Login(new LoginCommand(), default);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var user = await _userFactory.CreateAndSaveTestUser();
            var command = new LoginCommand { Email = user.Email, Password = "wrongpassword" };

            _mockHandler.Setup(h => h.Handle(It.IsAny<LoginCommand>(), default))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Act
            var result = await _controller.Login(command, default);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var user = await _userFactory.CreateAndSaveTestUser(password: "password");
            var command = new LoginCommand { Email = user.Email, Password = "password" };
            var expectedResponse = new LoginCommandResponse { Token = "test_token" };

            _mockHandler.Setup(h => h.Handle(It.IsAny<LoginCommand>(), default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(command, default);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var response = okResult.Value.ShouldBeOfType<LoginCommandResponse>();
            response.Token.ShouldBe("test_token");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 