using Api.Controllers.Users.UserSettings.Domain;
using Api.Controllers.Users.UserSettings.Http;
using Core.Models.Users;
using Core.Models.Users.Settings;
using Core.Services;
using Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Tests.Factories;

namespace Tests.Api.Tests.Controllers.Users;

public class UserSettingsControllerTests : IDisposable
{
    private readonly Mock<IGetUserSettingsService> _mockGetUserSettingsService;
    private readonly Mock<IUpdateUserSettingsService> _mockUpdateUserSettingsService;
    private readonly Mock<ILogger<UserSettingsController>> _mockLogger;
    private readonly UserSettingsController _controller;
    private readonly Context _context;
    private readonly TestUserFactory _testUserFactory;
    private readonly Guid _testUserId;
    private readonly IPasswordHasher _passwordHasher;

    public UserSettingsControllerTests()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new Context(options);
        _passwordHasher = new PasswordHasher();

        _mockGetUserSettingsService = new Mock<IGetUserSettingsService>();
        _mockUpdateUserSettingsService = new Mock<IUpdateUserSettingsService>();
        _mockLogger = new Mock<ILogger<UserSettingsController>>();
        _controller = new UserSettingsController(
            _mockGetUserSettingsService.Object,
            _mockUpdateUserSettingsService.Object,
            _mockLogger.Object);

        _testUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("sub", _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _testUserFactory = new TestUserFactory(_passwordHasher, _context);
    }

    [Fact(DisplayName = "GetUserSettings returns settings when they exist")]
    public async Task GetUserSettings_WhenSettingsExist_ReturnsSettings()
    {
        // Arrange
        var settings = new UserSettings
        {
            UserId = _testUserId,
            Language = "en",
            Theme = "dark",
            EmailNotifications = true,
            MeasurementSystem = "metric",
            TimeZone = "UTC"
        };

        _mockGetUserSettingsService
            .Setup(x => x.ExecuteAsync(It.IsAny<GetUserSettingsRequest>()))
            .ReturnsAsync(new GetUserSettingsResponse { Settings = settings });

        // Act
        var result = await _controller.GetUserSettings();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUserSettingsResponse>(okResult.Value);
        Assert.Equal(_testUserId, response.Settings.UserId);
        Assert.Equal("en", response.Settings.Language);
        Assert.Equal("dark", response.Settings.Theme);
    }

    [Fact(DisplayName = "GetUserSettings returns NotFound when settings don't exist")]
    public async Task GetUserSettings_WhenSettingsDontExist_ReturnsNotFound()
    {
        // Arrange
        _mockGetUserSettingsService
            .Setup(x => x.ExecuteAsync(It.IsAny<GetUserSettingsRequest>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.GetUserSettings();

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact(DisplayName = "UpdateUserSettings updates settings successfully")]
    public async Task UpdateUserSettings_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new UpdateUserSettingsRequest
        {
            Language = "fr",
            Theme = "light",
            EmailNotifications = false,
            MeasurementSystem = "imperial",
            TimeZone = "UTC+1"
        };

        _mockUpdateUserSettingsService
            .Setup(x => x.ExecuteAsync(It.IsAny<UpdateUserSettingsRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateUserSettings(request);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockUpdateUserSettingsService.Verify(
            x => x.ExecuteAsync(It.Is<UpdateUserSettingsRequest>(r => 
                r.UserId == _testUserId &&
                r.Language == "fr" &&
                r.Theme == "light" &&
                r.EmailNotifications == false &&
                r.MeasurementSystem == "imperial" &&
                r.TimeZone == "UTC+1")),
            Times.Once);
    }

    [Fact(DisplayName = "UpdateUserSettings returns 500 on error")]
    public async Task UpdateUserSettings_WhenErrorOccurs_Returns500()
    {
        // Arrange
        _mockUpdateUserSettingsService
            .Setup(x => x.ExecuteAsync(It.IsAny<UpdateUserSettingsRequest>()))
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var result = await _controller.UpdateUserSettings(new UpdateUserSettingsRequest());

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 