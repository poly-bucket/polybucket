using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.GetUserCollections.Http;
using PolyBucket.Api.Features.Collections.GetUserCollections.Repository;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections.GetUserCollections;

public class GetUserCollectionsControllerTests
{
    private readonly Mock<ICollectionRepository> _repository = new();
    private readonly Guid _userId = Guid.NewGuid();

    private GetUserCollectionsController CreateController(Guid? userId) =>
        new GetUserCollectionsController(_repository.Object).WithUser(userId);

    [Fact(DisplayName = "When the current user requests collections, GetCurrentUserCollections returns Ok with paging.")]
    public async Task GetCurrentUserCollections_Valid_ReturnsOk()
    {
        // Arrange
        var collections = new List<Collection>();
        _repository.Setup(r => r.GetCollectionsByUserIdAsync(_userId, 1, 12, null))
            .ReturnsAsync((collections, 0));

        // Act
        var result = await CreateController(_userId).GetCurrentUserCollections();

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the current user has no id claim, GetCurrentUserCollections returns Unauthorized.")]
    public async Task GetCurrentUserCollections_NoUser_ReturnsUnauthorized()
    {
        // Act
        var result = await CreateController(null).GetCurrentUserCollections();

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
        _repository.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When collections are requested by user id, GetCollectionsByUserId returns Ok.")]
    public async Task GetCollectionsByUserId_ReturnsOk()
    {
        // Arrange
        _repository.Setup(r => r.GetCollectionsByUserIdAsync(_userId, 1, 12, null))
            .ReturnsAsync((new List<Collection>(), 2));

        // Act
        var result = await CreateController(null).GetCollectionsByUserId(_userId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
