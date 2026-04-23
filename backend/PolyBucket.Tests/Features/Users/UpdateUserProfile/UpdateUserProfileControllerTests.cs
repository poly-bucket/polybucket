using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Domain;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Http;
using Xunit;

namespace PolyBucket.Tests.Features.Users.UpdateUserProfile;

public class UpdateUserProfileControllerTests
{
    private readonly Mock<IUpdateUserProfileService> _mockService;
    private readonly Mock<ILogger<UpdateUserProfileController>> _mockLogger;
    private readonly UpdateUserProfileController _controller;

    public UpdateUserProfileControllerTests()
    {
        _mockService = new Mock<IUpdateUserProfileService>();
        _mockLogger = new Mock<ILogger<UpdateUserProfileController>>();
        _controller = new UpdateUserProfileController(_mockService.Object, _mockLogger.Object);

        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }

    [Fact]
    public async Task UpdateUserProfile_ValidRequest_ReturnsOkResult()
    {
        var request = new UpdateUserProfileRequest
        {
            Bio = "Updated bio",
            Country = "Canada",
            WebsiteUrl = "https://example.com",
            TwitterUrl = "https://twitter.com/testuser",
            IsProfilePublic = true,
            ShowEmail = false,
            ShowLastLogin = true,
            ShowStatistics = true
        };

        _mockService
            .Setup(s => s.UpdateAsync(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic response = okResult.Value!;
        Assert.Equal("User profile updated successfully", (string)response.message);
    }

    [Fact]
    public async Task UpdateUserProfile_UserNotFound_ReturnsNotFound()
    {
        var request = new UpdateUserProfileRequest
        {
            Bio = "Updated bio",
            Country = "Canada"
        };

        _mockService
            .Setup(s => s.UpdateAsync(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        dynamic errorResponse = notFoundResult.Value!;
        Assert.Equal("User not found", (string)errorResponse.message);
    }

    [Fact]
    public async Task UpdateUserProfile_UnauthorizedAccess_ReturnsForbid()
    {
        var request = new UpdateUserProfileRequest
        {
            Bio = "Updated bio"
        };

        _mockService
            .Setup(s => s.UpdateAsync(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized access"));

        var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateUserProfile_Exception_ReturnsInternalServerError()
    {
        var request = new UpdateUserProfileRequest
        {
            Bio = "Updated bio"
        };

        _mockService
            .Setup(s => s.UpdateAsync(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        dynamic errorResponse = statusCodeResult.Value!;
        Assert.Equal("An error occurred while updating the user profile", (string)errorResponse.message);
    }

    [Fact]
    public async Task UpdateUserProfile_InvalidUserId_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim("no-id", "x")], "Test"))
            }
        };

        var request = new UpdateUserProfileRequest
        {
            Bio = "Updated bio"
        };

        var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }
}
