using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetUserProfile.Domain;
using PolyBucket.Api.Features.Users.GetUserProfile.Http;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserProfile;

public class GetUserProfileControllerTests
{
    private readonly Mock<IGetUserProfileService> _mockService;
    private readonly Mock<ILogger<GetUserProfileController>> _mockLogger;
    private readonly GetUserProfileController _controller;

    public GetUserProfileControllerTests()
    {
        _mockService = new Mock<IGetUserProfileService>();
        _mockLogger = new Mock<ILogger<GetUserProfileController>>();
        _controller = new GetUserProfileController(_mockService.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "When getting a user profile by a valid id, the get user profile controller returns Ok with the profile.")]
    public async Task GetUserProfileById_ValidId_ReturnsOkResult()
    {
        var userId = Guid.NewGuid();
        var expectedResponse = new GetUserProfileResponse
        {
            Id = userId,
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Bio = "Test bio",
            Avatar = "avatar.jpg",
            Country = "US",
            RoleName = "User",
            CreatedAt = DateTime.UtcNow,
            TotalModels = 5,
            TotalCollections = 2,
            TotalLikes = 10,
            TotalDownloads = 25
        };

        _mockService
            .Setup(s => s.GetUserProfileAsync(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetUserProfileById(userId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUserProfileResponse>(okResult.Value);
        Assert.Equal(userId, response.Id);
        Assert.Equal("testuser", response.Username);
        Assert.Equal(5, response.TotalModels);
    }

    [Fact(DisplayName = "When getting a user profile by id and the user is not found, the get user profile controller returns NotFound.")]
    public async Task GetUserProfileById_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        _mockService
            .Setup(s => s.GetUserProfileAsync(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var result = await _controller.GetUserProfileById(userId, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var messageProperty = Assert.IsType<string>(notFoundResult.Value?.GetType().GetProperty("message")?.GetValue(notFoundResult.Value));
        Assert.Equal("User profile not found", messageProperty);
    }

    [Fact(DisplayName = "When getting a user profile by id and an unexpected exception is thrown, the get user profile controller returns InternalServerError.")]
    public async Task GetUserProfileById_Exception_ReturnsInternalServerError()
    {
        var userId = Guid.NewGuid();
        _mockService
            .Setup(s => s.GetUserProfileAsync(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetUserProfileById(userId, CancellationToken.None);

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var messageProperty = Assert.IsType<string>(statusCodeResult.Value?.GetType().GetProperty("message")?.GetValue(statusCodeResult.Value));
        Assert.Equal("An error occurred while retrieving the user profile", messageProperty);
    }

    [Fact(DisplayName = "When getting a user profile by a valid username, the get user profile controller returns Ok with the profile.")]
    public async Task GetUserProfileByUsername_ValidUsername_ReturnsOkResult()
    {
        var username = "testuser";
        var expectedResponse = new GetUserProfileResponse
        {
            Id = Guid.NewGuid(),
            Username = username,
            FirstName = "Test",
            LastName = "User",
            Bio = "Test bio",
            Avatar = "avatar.jpg",
            Country = "US",
            RoleName = "User",
            CreatedAt = DateTime.UtcNow,
            TotalModels = 3,
            TotalCollections = 1,
            TotalLikes = 8,
            TotalDownloads = 15
        };

        _mockService
            .Setup(s => s.GetUserProfileAsync(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetUserProfileByUsername(username, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUserProfileResponse>(okResult.Value);
        Assert.Equal(username, response.Username);
        Assert.Equal(3, response.TotalModels);
    }

    [Fact(DisplayName = "When getting a user profile by username and the user is not found, the get user profile controller returns NotFound.")]
    public async Task GetUserProfileByUsername_UserNotFound_ReturnsNotFound()
    {
        var username = "nonexistentuser";
        _mockService
            .Setup(s => s.GetUserProfileAsync(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var result = await _controller.GetUserProfileByUsername(username, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var messageProperty = Assert.IsType<string>(notFoundResult.Value?.GetType().GetProperty("message")?.GetValue(notFoundResult.Value));
        Assert.Equal("User profile not found", messageProperty);
    }

    [Fact(DisplayName = "When getting a user profile by username and an unexpected exception is thrown, the get user profile controller returns InternalServerError.")]
    public async Task GetUserProfileByUsername_Exception_ReturnsInternalServerError()
    {
        var username = "testuser";
        _mockService
            .Setup(s => s.GetUserProfileAsync(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetUserProfileByUsername(username, CancellationToken.None);

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var messageProperty = Assert.IsType<string>(statusCodeResult.Value?.GetType().GetProperty("message")?.GetValue(statusCodeResult.Value));
        Assert.Equal("An error occurred while retrieving the user profile", messageProperty);
    }
}
