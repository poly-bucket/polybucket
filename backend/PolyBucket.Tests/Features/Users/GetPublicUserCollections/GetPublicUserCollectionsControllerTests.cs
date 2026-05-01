using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetPublicUserCollections.Domain;
using PolyBucket.Api.Features.Users.GetPublicUserCollections.Http;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetPublicUserCollections;

public class GetPublicUserCollectionsControllerTests
{
    private readonly Mock<IGetPublicUserCollectionsService> _serviceMock;
    private readonly GetPublicUserCollectionsController _controller;

    public GetPublicUserCollectionsControllerTests()
    {
        _serviceMock = new Mock<IGetPublicUserCollectionsService>();
        var logger = new Mock<ILogger<GetPublicUserCollectionsController>>();
        _controller = new GetPublicUserCollectionsController(_serviceMock.Object, logger.Object);
    }

    [Fact(DisplayName = "When requester is admin, collections by username includes admin context in query.")]
    public async Task GetPublicUserCollectionsByUsername_AdminUser_PassesRequesterContext()
    {
        // Arrange
        var expected = new GetPublicUserCollectionsResult { TotalCount = 1, Page = 1, PageSize = 20, TotalPages = 1 };
        _serviceMock
            .Setup(s => s.GetPublicUserCollectionsAsync(It.IsAny<GetPublicUserCollectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        SetUser(Guid.NewGuid(), isAdmin: true);

        // Act
        var response = await _controller.GetPublicUserCollectionsByUsername("admin", cancellationToken: CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, ok.Value);
        _serviceMock.Verify(s => s.GetPublicUserCollectionsAsync(
            It.Is<GetPublicUserCollectionsQuery>(q =>
                q.Username == "admin" &&
                q.IsRequestingUserAdmin &&
                q.RequestingUserId.HasValue),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When target user is missing, collections by id returns not found.")]
    public async Task GetPublicUserCollections_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _serviceMock
            .Setup(s => s.GetPublicUserCollectionsAsync(It.IsAny<GetPublicUserCollectionsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var response = await _controller.GetPublicUserCollections(userId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact(DisplayName = "When profile is private for non-owner viewer, collections endpoint returns forbidden.")]
    public async Task GetPublicUserCollections_PrivateProfile_ReturnsForbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _serviceMock
            .Setup(s => s.GetPublicUserCollectionsAsync(It.IsAny<GetPublicUserCollectionsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User profile is private"));

        // Act
        var response = await _controller.GetPublicUserCollections(userId, cancellationToken: CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(403, objectResult.StatusCode);
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
