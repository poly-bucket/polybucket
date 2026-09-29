using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserLikedModels;

public class GetUserLikedModelsControllerTests
{
    private readonly Mock<IGetUserLikedModelsService> _mockService;
    private readonly Mock<ILogger<GetUserLikedModelsController>> _mockLogger;
    private readonly GetUserLikedModelsController _controller;

    public GetUserLikedModelsControllerTests()
    {
        _mockService = new Mock<IGetUserLikedModelsService>();
        _mockLogger = new Mock<ILogger<GetUserLikedModelsController>>();
        _controller = new GetUserLikedModelsController(_mockService.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "When getting liked models by user id, the get user liked models controller returns Ok.")]
    public async Task GetUserLikedModels_ValidUserId_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        _mockService
            .Setup(s => s.GetUserLikedModelsAsync(It.IsAny<GetUserLikedModelsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUserLikedModelsResult { TotalCount = 0, Page = 1, PageSize = 20 });

        var result = await _controller.GetUserLikedModels(userId, 1, 20, null, null, true, CancellationToken.None);

        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
