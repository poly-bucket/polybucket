using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.LikeComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class LikeCommentController(ICommentReactionService reactionService) : ControllerBase
{
    /// <summary>
    /// Likes a comment. Liking twice has no further effect, and liking a disliked comment switches the reaction.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The comment's counts and the caller's current reaction.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="404">The comment does not exist or is hidden.</response>
    [HttpPost("{commentId:guid}/like")]
    [ProducesResponseType(typeof(CommentReactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LikeComment(Guid commentId, CancellationToken cancellationToken)
    {
        var userId = User.GetCommentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var outcome = await reactionService.ReactAsync(commentId, userId.Value, CommentReactionType.Like, cancellationToken);
        return outcome.Change == CommentReactionChange.NotFound
            ? NotFound()
            : Ok(CommentReactionResponse.From(outcome));
    }
}
