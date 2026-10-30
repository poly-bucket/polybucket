using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.LikeComment.Domain;
using PolyBucket.Api.Features.Comments.LikeComment.Http;

namespace PolyBucket.Api.Features.Comments.LikeComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class LikeCommentController(ILikeCommentService likeCommentService) : ControllerBase
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

        var outcome = await likeCommentService.ReactAsync(commentId, userId.Value, cancellationToken);
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
