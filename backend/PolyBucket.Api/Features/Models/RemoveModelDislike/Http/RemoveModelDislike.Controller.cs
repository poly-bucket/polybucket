using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.Http;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.RemoveModelDislike.Http;

[Authorize]
[ApiController]
[Route("api/models")]
public class RemoveModelDislikeController(IModelReactionService reactionService) : ControllerBase
{
    /// <summary>
    /// Removes the caller's dislike from a model. Does nothing if the caller has not disliked it.
    /// </summary>
    [HttpDelete("{id:guid}/dislike")]
    [ProducesResponseType(typeof(ModelReactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDislike(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await reactionService.RemoveReactionAsync(id, User, ModelReactionType.Dislike, cancellationToken);
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
