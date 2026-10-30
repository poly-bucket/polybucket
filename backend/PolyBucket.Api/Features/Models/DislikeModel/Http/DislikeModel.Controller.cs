using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.DislikeModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Http;

namespace PolyBucket.Api.Features.Models.DislikeModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
public class DislikeModelController(IDislikeModelService service) : ControllerBase
{
    [HttpPost("{id:guid}/dislike")]
    [ProducesResponseType(typeof(ModelReactionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DislikeModel(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await service.DislikeAsync(id, User, cancellationToken);
        return outcome.Change switch
        {
            ModelReactionChange.NotFound => NotFound(),
            ModelReactionChange.Forbidden => Forbid(),
            ModelReactionChange.Disabled => Forbid(),
            ModelReactionChange.Unauthorized => Unauthorized(),
            _ => Ok(ModelReactionResponse.From(outcome))
        };
    }
}
