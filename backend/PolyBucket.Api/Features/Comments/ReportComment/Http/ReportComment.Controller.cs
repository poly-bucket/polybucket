using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.ReportComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class ReportCommentController(IEnhancedCommentsPlugin commentsPlugin) : ControllerBase
{
    /// <summary>
    /// Reports a comment to the moderators.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="request">Why the comment is being reported.</param>
    /// <response code="200">The report was submitted.</response>
    /// <response code="400">The report could not be submitted.</response>
    /// <response code="401">The caller is not signed in.</response>
    [HttpPost("{commentId:guid}/report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReportComment(Guid commentId, [FromBody] ReportCommentRequest request)
    {
        var userId = User.GetCommentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var reported = await commentsPlugin.ReportCommentAsync(commentId, userId.Value, request.Reason.Trim());
        return reported
            ? Ok(new { message = "Comment reported successfully" })
            : BadRequest(new { message = "Unable to report comment" });
    }
}
