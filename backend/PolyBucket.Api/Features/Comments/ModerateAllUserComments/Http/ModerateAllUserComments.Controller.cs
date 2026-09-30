using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.ModerateComment.Http;

namespace PolyBucket.Api.Features.Comments.ModerateAllUserComments.Http;

[ApiController]
[Route("api/comments")]
[Authorize(Roles = "Admin,Moderator")]
public class ModerateAllUserCommentsController(IEnhancedCommentsPlugin commentsPlugin) : ControllerBase
{
    /// <summary>
    /// Hides every comment written by a user.
    /// </summary>
    /// <param name="targetUserId">The user whose comments are hidden.</param>
    /// <param name="request">The moderation reason.</param>
    /// <response code="200">The comments were moderated.</response>
    /// <response code="400">The comments could not be moderated.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The caller is not an admin or moderator.</response>
    [HttpPost("user/{targetUserId:guid}/moderate-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ModerateAllUserComments(Guid targetUserId, [FromBody] ModerateCommentRequest request)
    {
        var moderatorId = User.GetCommentUserId();
        if (moderatorId == null)
        {
            return Unauthorized();
        }

        var moderated = await commentsPlugin.ModerateAllCommentsForUserAsync(targetUserId, moderatorId.Value, request.Reason.Trim());
        return moderated
            ? Ok(new { message = "All user comments moderated successfully" })
            : BadRequest(new { message = "Unable to moderate user comments" });
    }
}
