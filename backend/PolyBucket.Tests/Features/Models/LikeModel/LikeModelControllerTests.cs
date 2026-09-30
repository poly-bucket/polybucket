using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Models.Http;
using PolyBucket.Api.Features.Models.LikeModel.Http;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;
using PolyBucket.Tests.Features.Comments;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.LikeModel;

public class LikeModelControllerTests
{
    private readonly Mock<IModelReactionService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When liking a model, the controller returns Ok with reaction counts.")]
    public async Task LikeModel_ReturnsOkWithBody()
    {
        var modelId = Guid.NewGuid();
        _service.Setup(s => s.ReactAsync(modelId, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), ModelReactionType.Like, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelReactionOutcome(ModelReactionChange.Applied, 1, 0, ModelReactionType.Like));
        var controller = new LikeModelController(_service.Object).WithUser(_userId);

        var result = await controller.LikeModel(modelId, CancellationToken.None);

        var body = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<ModelReactionResponse>();
        body.Likes.ShouldBe(1);
        body.UserHasLiked.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the author likes their own model, the controller returns Forbid.")]
    public async Task LikeModel_SelfReaction_ReturnsForbid()
    {
        var modelId = Guid.NewGuid();
        _service.Setup(s => s.ReactAsync(modelId, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), ModelReactionType.Like, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModelReactionOutcome.Forbidden);
        var controller = new LikeModelController(_service.Object).WithUser(_userId);

        var result = await controller.LikeModel(modelId, CancellationToken.None);

        result.ShouldBeOfType<ForbidResult>();
    }
}
