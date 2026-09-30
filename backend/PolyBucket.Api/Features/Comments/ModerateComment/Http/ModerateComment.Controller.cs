using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.ModerateComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize(Roles = "Admin,Moderator")]
public class ModerateCommentController(IEnhancedCommentsPlugin commentsPlugin) : ControllerBase
{
    /// <summary>
    /// Hides a comment for moderation, recording who hid it and why.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="request">The moderation reason.</param>
    /// <response code="200">The comment was moderated.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The caller is not an admin or moderator.</response>
    /// <response code="404">The comment does not exist.</response>
    [HttpPost("{commentId:guid}/moderate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModerateComment(Guid commentId, [FromBody] ModerateCommentRequest request)
    {
        var moderatorId = User.GetCommentUserId();
        if (moderatorId == null)
        {
            return Unauthorized();
        }

        var moderated = await commentsPlugin.ModerateCommentAsync(commentId, moderatorId.Value, request.Reason.Trim());
        return moderated
            ? Ok(new { message = "Comment moderated successfully" })
            : NotFound();
    }
}
