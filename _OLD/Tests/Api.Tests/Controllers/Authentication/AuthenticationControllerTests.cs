using Api.Controllers.Authentication.Domain;
using Api.Controllers.Authentication.Http;
using Api.Controllers.Authentication.Persistance;
using Core.Services;
using Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Net;
using System.Net.Http;
using Tests.Factories;

namespace Tests.Api.Tests.Controllers.Authentication;

public class AuthenticationControllerTests : IDisposable
{
    private readonly Mock<CreateUserLoginDataAccess> _mockCreateUserLoginDataAccess;
    private readonly Mock<CreateUserLoginService> _mockLoginService;
    private readonly Mock<ILogger<CreateUserLoginService>> _mockCreateUserLoginServiceLogger;
    private readonly Mock<ILogger<AuthenticationController>> _mockAuthenticationControllerLogger;
    private readonly AuthenticationController _controller;
    private readonly IPasswordHasher _passwordHasher;
    private readonly Context _context;
    private readonly TestUserFactory _testUserFactory;

    public AuthenticationControllerTests()
    {
        // Set up environment variables
        Environment.SetEnvironmentVariable("JWT_SECRET", "test_jwt_secret_key_for_testing_purposes_only");
        Environment.SetEnvironmentVariable("MYSQL_HOST", "localhost");
        Environment.SetEnvironmentVariable("MYSQL_PORT", "3306");
        Environment.SetEnvironmentVariable("MYSQL_DATABASE", "TestDatabase");
        Environment.SetEnvironmentVariable("MYSQL_USER", "test_user");
        Environment.SetEnvironmentVariable("MYSQL_PASSWORD", "test_password");

        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new Context(options);

        _passwordHasher = new PasswordHasher();
        _mockCreateUserLoginDataAccess = new Mock<CreateUserLoginDataAccess>(_context, null, null);
        _mockCreateUserLoginServiceLogger = new Mock<ILogger<CreateUserLoginService>>();
        _mockAuthenticationControllerLogger = new Mock<ILogger<AuthenticationController>>();
        _mockLoginService = new Mock<CreateUserLoginService>(_mockCreateUserLoginDataAccess.Object, _passwordHasher, _mockCreateUserLoginServiceLogger.Object);
        _controller = new AuthenticationController(_mockLoginService.Object, _mockAuthenticationControllerLogger.Object);

        // Setup default HttpContext for IP address testing
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _testUserFactory = new TestUserFactory(_passwordHasher, _context);
    }

    [Fact(DisplayName = "Invalid Reqest Returns BadRequest (400)")]
    public async Task Login_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Login(new CreateUserLoginRequest());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "Invalid Credentials Returns Unauthorized (401)")]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var testUser = await _testUserFactory.CreateAndSaveTestUser();

        var loginRequest = new CreateUserLoginRequest
        {
            Email = testUser.Email,
            Password = "WrongPassword123!",
            UserAgent = "TestAgent",
        };

        // Act
        var result = await _controller.Login(loginRequest);
        var loginAttempt = await _context.UserLogins.Where(x => x.Email == loginRequest.Email).FirstOrDefaultAsync();

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(loginAttempt);
        Assert.Equal(loginAttempt.Email, loginRequest.Email);
        Assert.Equal(loginAttempt.Successful, false);
        Assert.Equal(loginAttempt.IpAddress, _controller.HttpContext.Connection.RemoteIpAddress?.ToString());
        Assert.Equal(loginAttempt.UserAgent, loginRequest.UserAgent);
    }

    [Fact(DisplayName = "Valid Credentials Returns Token (200)")]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        string password = "Password123!";
        string email = "admin@localhost.local";
        var testUser = await _testUserFactory.CreateAndSaveTestUser(email: email, password: password);
        var loginRequest = new CreateUserLoginRequest
        {
            Email = email,
            Password = password,
            UserAgent = "TestAgent"
        };

        // Act
        var result = await _controller.Login(loginRequest);
        var loginAttempt = await _context.UserLogins.Where(x => x.Email == loginRequest.Email).FirstOrDefaultAsync();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(loginAttempt);
        Assert.Equal(loginAttempt.Email, loginRequest.Email);
        Assert.Equal(loginAttempt.Successful, true);
        Assert.Equal(loginAttempt.IpAddress, _controller.HttpContext.Connection.RemoteIpAddress?.ToString());
        Assert.Equal(loginAttempt.UserAgent, loginRequest.UserAgent);
    }

    public void Dispose()
    {
        // Clean up environment variables
        Environment.SetEnvironmentVariable("JWT_SECRET", null);
        Environment.SetEnvironmentVariable("MYSQL_HOST", null);
        Environment.SetEnvironmentVariable("MYSQL_PORT", null);
        Environment.SetEnvironmentVariable("MYSQL_DATABASE", null);
        Environment.SetEnvironmentVariable("MYSQL_USER", null);
        Environment.SetEnvironmentVariable("MYSQL_PASSWORD", null);

        _context.Dispose();
    }
}