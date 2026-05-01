using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetUserModels.Domain;
using PolyBucket.Api.Features.Users.GetUserModels.Http;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserModels;

public class GetUserModelsControllerTests
{
    private readonly Mock<IGetUserModelsService> _serviceMock;
    private readonly GetUserModelsController _controller;

    public GetUserModelsControllerTests()
    {
        _serviceMock = new Mock<IGetUserModelsService>();
        var logger = new Mock<ILogger<GetUserModelsController>>();
        _controller = new GetUserModelsController(_serviceMock.Object, logger.Object);
    }

    [Fact(DisplayName = "When requester is admin, model query enables private visibility.")]
    public async Task GetUserPublicModels_AdminUser_SetsIncludePrivate()
    {
        // Arrange
        var expected = new GetUserModelsResult { TotalCount = 1, Page = 1, PageSize = 20, TotalPages = 1 };
        _serviceMock
            .Setup(s => s.GetUserPublicModelsAsync(It.IsAny<GetUserModelsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        SetUser(Guid.NewGuid(), isAdmin: true);

        // Act
        var response = await _controller.GetUserPublicModels("admin", cancellationToken: CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, ok.Value);
        _serviceMock.Verify(s => s.GetUserPublicModelsAsync(
            It.Is<GetUserModelsQuery>(q =>
                q.Username == "admin" &&
                q.IncludePrivate &&
                q.IsRequestingUserAdmin &&
                q.RequestingUserId.HasValue),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When profile is private for non-owner viewer, models endpoint returns forbidden.")]
    public async Task GetUserPublicModels_PrivateProfile_ReturnsForbidden()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetUserPublicModelsAsync(It.IsAny<GetUserModelsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User profile is private"));

        // Act
        var response = await _controller.GetUserPublicModels("private-user", cancellationToken: CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact(DisplayName = "When user is not found, models endpoint returns not found.")]
    public async Task GetUserPublicModels_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetUserPublicModelsAsync(It.IsAny<GetUserModelsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("User models not found"));

        // Act
        var response = await _controller.GetUserPublicModels("missing-user", cancellationToken: CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    private void SetUser(Guid userId, bool isAdmin)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}
