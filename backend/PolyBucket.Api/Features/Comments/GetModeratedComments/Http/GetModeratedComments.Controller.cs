using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.GetModeratedComments.Http;

[ApiController]
[Route("api/comments")]
[Authorize(Roles = "Admin,Moderator")]
public class GetModeratedCommentsController(IEnhancedCommentsPlugin commentsPlugin, ICommentResponseMapper mapper) : ControllerBase
{
    /// <summary>
    /// Gets moderated comments, most recently moderated first.
    /// </summary>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Comments per page, 1 to 100.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The moderated comments.</response>
    /// <response code="400">The paging values are invalid.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The caller is not an admin or moderator.</response>
    [HttpGet("moderated")]
    [ProducesResponseType(typeof(List<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetModeratedComments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > CommentLimits.MaxPageSize)
        {
            return BadRequest(new { message = $"Page must be at least 1 and page size between 1 and {CommentLimits.MaxPageSize}" });
        }

        var comments = (await commentsPlugin.GetModeratedCommentsAsync(page, pageSize)).ToList();
        var mapped = await mapper.MapAsync(comments, User.GetCommentUserId(), User.IsCommentAdmin(), includeReplies: false, cancellationToken);
        return Ok(mapped);
    }
}
