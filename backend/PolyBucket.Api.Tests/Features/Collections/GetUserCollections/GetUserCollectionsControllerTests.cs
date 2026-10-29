using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.GetUserCollections.Domain;
using PolyBucket.Api.Features.Collections.GetUserCollections.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Collections.GetUserCollections;

public class GetUserCollectionsControllerTests
{
    private readonly Mock<IGetUserCollectionsService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    private GetUserCollectionsController CreateController(Guid? userId) =>
        new GetUserCollectionsController(_service.Object).WithUser(userId);

    [Fact(DisplayName = "When the current user requests collections, GetCurrentUserCollections returns Ok with paging.")]
    public async Task GetCurrentUserCollections_Valid_ReturnsOk()
    {
        // Arrange
        var response = new GetUserCollectionsResponseDto { Page = 1, PageSize = 12 };
        _service.Setup(s => s.GetUserCollectionsAsync(It.IsAny<GetUserCollectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await CreateController(_userId).GetCurrentUserCollections();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When the current user has no id claim, GetCurrentUserCollections returns Unauthorized.")]
    public async Task GetCurrentUserCollections_NoUser_ReturnsUnauthorized()
    {
        // Act
        var result = await CreateController(null).GetCurrentUserCollections();

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        _service.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When collections are requested by user id, GetCollectionsByUserId returns Ok.")]
    public async Task GetCollectionsByUserId_ReturnsOk()
    {
        // Arrange
        var response = new GetUserCollectionsResponseDto { TotalCount = 2 };
        _service.Setup(s => s.GetUserCollectionsAsync(It.IsAny<GetUserCollectionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await CreateController(null).GetCollectionsByUserId(_userId);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When the target user profile is private, GetCollectionsByUserId returns Forbidden.")]
    public async Task GetCollectionsByUserId_PrivateProfile_ReturnsForbidden()
    {
        // Arrange
        _service.Setup(s => s.GetUserCollectionsAsync(It.IsAny<GetUserCollectionsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await CreateController(null).GetCollectionsByUserId(_userId);

        // Assert
        result.Result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(403);
    }
}
