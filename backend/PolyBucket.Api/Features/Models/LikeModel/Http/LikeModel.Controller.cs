using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.Http;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.LikeModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
public class LikeModelController(IModelReactionService reactionService) : ControllerBase
{
    /// <summary>
    /// Likes a model. Liking twice has no further effect, and liking a disliked model switches the reaction.
    /// </summary>
    [HttpPost("{id:guid}/like")]
    [ProducesResponseType(typeof(ModelReactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LikeModel(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await reactionService.ReactAsync(id, User, ModelReactionType.Like, cancellationToken);
        return MapOutcome(outcome);
    }

    /// <summary>
    /// Removes the caller's like from a model. Does nothing if the caller has not liked it.
    /// </summary>
    [HttpDelete("{id:guid}/like")]
    [ProducesResponseType(typeof(ModelReactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlikeModel(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await reactionService.RemoveReactionAsync(id, User, ModelReactionType.Like, cancellationToken);
        return MapOutcome(outcome);
    }

    private IActionResult MapOutcome(ModelReactionOutcome outcome) =>
        outcome.Change switch
        {
            ModelReactionChange.NotFound => NotFound(),
            ModelReactionChange.Forbidden => Forbid(),
            ModelReactionChange.Disabled => Forbid(),
            ModelReactionChange.Unauthorized => Unauthorized(),
            _ => Ok(ModelReactionResponse.From(outcome))
        };
}
