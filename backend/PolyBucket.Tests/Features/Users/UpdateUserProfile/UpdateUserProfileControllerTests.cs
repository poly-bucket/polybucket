using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Domain;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Http;
using System.Security.Claims;

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

    [Fact(DisplayName = "When updating a user profile with a valid request, the update user profile controller returns Ok.")]
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
        var messageProperty = okResult.Value!.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("User profile updated successfully", messageProperty!.GetValue(okResult.Value) as string);
    }

    [Fact(DisplayName = "When updating a user profile and the user is not found, the update user profile controller returns NotFound.")]
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
        var messageProperty = notFoundResult.Value!.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("User not found", messageProperty!.GetValue(notFoundResult.Value) as string);
    }

    [Fact(DisplayName = "When updating a user profile with unauthorized access, the update user profile controller returns Forbid.")]
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

    [Fact(DisplayName = "When updating a user profile and an unexpected exception is thrown, the update user profile controller returns InternalServerError.")]
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
        var messageProperty = statusCodeResult.Value!.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal(
            "An error occurred while updating the user profile",
            messageProperty!.GetValue(statusCodeResult.Value) as string);
    }

    [Fact(DisplayName = "When updating a user profile without a valid user id claim, the update user profile controller returns Unauthorized.")]
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
