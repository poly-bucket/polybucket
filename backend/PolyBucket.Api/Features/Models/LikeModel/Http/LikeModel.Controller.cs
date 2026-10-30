using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Http;

namespace PolyBucket.Api.Features.Models.LikeModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
public class LikeModelController(ILikeModelService service) : ControllerBase
{
    /// <summary>
    /// Likes a model. Liking twice has no further effect, and liking a disliked model switches the reaction.
    /// </summary>
    [HttpPost("{id:guid}/like")]
    [ProducesResponseType(typeof(ModelReactionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LikeModel(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await service.LikeAsync(id, User, cancellationToken);
        return MapOutcome(outcome);
    }

    private IActionResult MapOutcome(ModelReactionOutcome outcome) => outcome.Change switch
    {
        ModelReactionChange.NotFound => NotFound(),
        ModelReactionChange.Forbidden => Forbid(),
        ModelReactionChange.Disabled => Forbid(),
        ModelReactionChange.Unauthorized => Unauthorized(),
        _ => Ok(ModelReactionResponse.From(outcome))
    };
}
