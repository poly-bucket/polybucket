using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.RemoveCommentLike.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class RemoveCommentLikeController(ICommentReactionService reactionService) : ControllerBase
{
    /// <summary>
    /// Removes the caller's like from a comment. Does nothing if the caller has not liked it.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The comment's counts and the caller's current reaction.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="404">The comment does not exist or is hidden.</response>
    [HttpDelete("{commentId:guid}/like")]
    [ProducesResponseType(typeof(CommentReactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveLike(Guid commentId, CancellationToken cancellationToken)
    {
        var userId = User.GetCommentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var outcome = await reactionService.RemoveReactionAsync(commentId, userId.Value, CommentReactionType.Like, cancellationToken);
        return MapOutcome(outcome);
    }

    private IActionResult MapOutcome(CommentReactionOutcome outcome) =>
        outcome.Change switch
        {
            CommentReactionChange.NotFound => NotFound(),
            CommentReactionChange.Forbidden => Forbid(),
            _ => Ok(CommentReactionResponse.From(outcome))
        };
}
