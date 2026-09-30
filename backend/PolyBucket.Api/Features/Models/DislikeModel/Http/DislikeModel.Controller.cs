using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.Http;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DislikeModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
public class DislikeModelController(IModelReactionService reactionService) : ControllerBase
{
    /// <summary>
    /// Dislikes a model. Disliking twice has no further effect, and disliking a liked model switches the reaction.
    /// </summary>
    [HttpPost("{id:guid}/dislike")]
    [ProducesResponseType(typeof(ModelReactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DislikeModel(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await reactionService.ReactAsync(id, User, ModelReactionType.Dislike, cancellationToken);
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
