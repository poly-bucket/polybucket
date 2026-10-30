using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.GetCommentsForTarget.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class GetCommentsForTargetController(IGetCommentsForTargetService getCommentsForTargetService) : ControllerBase
{
    /// <summary>
    /// Gets a page of top-level comments for a model, user profile, or collection, each with its replies.
    /// </summary>
    /// <param name="targetType">model, user, userprofile, collection, or report.</param>
    /// <param name="targetId">The target's id.</param>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Comments per page, 1 to 100.</param>
    /// <param name="includeHidden">Include moderated comments; honored only for admins and moderators.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The page of comments with statistics.</response>
    /// <response code="400">The target type or paging values are invalid.</response>
    /// <response code="401">The caller is not signed in.</response>
    [HttpGet("target/{targetType}/{targetId:guid}")]
    [ProducesResponseType(typeof(CommentsPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCommentsForTarget(
        string targetType,
        Guid targetId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var result = await getCommentsForTargetService.GetAsync(
            targetType,
            targetId,
            page,
            pageSize,
            includeHidden,
            User.IsCommentModerator(),
            User.GetCommentUserId(),
            User.IsCommentAdmin(),
            cancellationToken);

        return result.ErrorMessage == null
            ? Ok(result.Page)
            : BadRequest(new { message = result.ErrorMessage });
    }
}
