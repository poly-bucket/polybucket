using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Models.DislikeModel.Http;
using PolyBucket.Api.Features.Models.Http;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;
using PolyBucket.Tests.Features.Comments;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DislikeModel;

public class DislikeModelControllerTests
{
    private readonly Mock<IModelReactionService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When disliking a model, the controller returns Ok with dislike counts.")]
    public async Task DislikeModel_ReturnsOkWithBody()
    {
        var modelId = Guid.NewGuid();
        _service.Setup(s => s.ReactAsync(modelId, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), ModelReactionType.Dislike, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelReactionOutcome(ModelReactionChange.Applied, 0, 1, ModelReactionType.Dislike));
        var controller = new DislikeModelController(_service.Object).WithUser(_userId);

        var result = await controller.DislikeModel(modelId, CancellationToken.None);

        var body = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<ModelReactionResponse>();
        body.Dislikes.ShouldBe(1);
        body.UserHasDisliked.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the model is missing, the controller returns NotFound.")]
    public async Task DislikeModel_Missing_ReturnsNotFound()
    {
        _service.Setup(s => s.ReactAsync(It.IsAny<Guid>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), ModelReactionType.Dislike, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModelReactionOutcome.NotFound);
        var controller = new DislikeModelController(_service.Object).WithUser(_userId);

        var result = await controller.DislikeModel(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeOfType<NotFoundResult>();
    }
}
