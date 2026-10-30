using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.GetModeratedComments.Domain;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.GetModeratedComments.Http;

[ApiController]
[Route("api/comments")]
[Authorize(Roles = "Admin,Moderator")]
public class GetModeratedCommentsController(IGetModeratedCommentsService getModeratedCommentsService) : ControllerBase
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
        var result = await getModeratedCommentsService.GetAsync(page, pageSize, User.GetCommentUserId(), User.IsCommentAdmin(), cancellationToken);
        return result.ErrorMessage == null
            ? Ok(result.Comments)
            : BadRequest(new { message = result.ErrorMessage });
    }
}
