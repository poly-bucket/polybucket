using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.UnmoderateComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize(Roles = "Admin,Moderator")]
public class UnmoderateCommentController(IEnhancedCommentsPlugin commentsPlugin) : ControllerBase
{
    /// <summary>
    /// Restores a moderated comment so it is visible again.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <response code="200">The comment was restored.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The caller is not an admin or moderator.</response>
    /// <response code="404">The comment does not exist.</response>
    [HttpPost("{commentId:guid}/unmoderate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnmoderateComment(Guid commentId)
    {
        var moderatorId = User.GetCommentUserId();
        if (moderatorId == null)
        {
            return Unauthorized();
        }

        var restored = await commentsPlugin.UnmoderateCommentAsync(commentId, moderatorId.Value);
        return restored
            ? Ok(new { message = "Comment unmoderated successfully" })
            : NotFound();
    }
}
