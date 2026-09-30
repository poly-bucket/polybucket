using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.GetCommentsForTarget.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class GetCommentsForTargetController(IEnhancedCommentsPlugin commentsPlugin, ICommentResponseMapper mapper) : ControllerBase
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
        if (!CommentTargetParser.TryParse(targetType, targetId, out var target))
        {
            return BadRequest(new { message = $"Invalid target type: {targetType}" });
        }

        if (page < 1 || pageSize < 1 || pageSize > CommentLimits.MaxPageSize)
        {
            return BadRequest(new { message = $"Page must be at least 1 and page size between 1 and {CommentLimits.MaxPageSize}" });
        }

        var showHidden = includeHidden && User.IsCommentModerator();
        var comments = (await commentsPlugin.GetCommentsForTargetAsync(target, showHidden, page, pageSize)).ToList();
        var totalCount = await commentsPlugin.GetCommentCountForTargetAsync(target, showHidden);
        var statistics = await commentsPlugin.GetCommentStatisticsAsync(target);
        var mapped = await mapper.MapAsync(comments, User.GetCommentUserId(), User.IsCommentAdmin(), includeReplies: true, cancellationToken);

        return Ok(new CommentsPagedResponse
        {
            Comments = mapped,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Statistics = statistics
        });
    }
}
