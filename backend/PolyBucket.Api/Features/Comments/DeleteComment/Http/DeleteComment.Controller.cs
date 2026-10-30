using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.DeleteComment.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.DeleteComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class DeleteCommentController(IDeleteCommentService deleteCommentService) : ControllerBase
{
    /// <summary>
    /// Deletes a comment and its replies. Authors can delete their own comments; admins can delete any comment.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">The comment was deleted.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="404">The comment does not exist or the caller cannot delete it.</response>
    [HttpDelete("{commentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken cancellationToken)
    {
        var userId = User.GetCommentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var deleted = await deleteCommentService.DeleteAsync(commentId, userId.Value, User.IsCommentAdmin(), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
