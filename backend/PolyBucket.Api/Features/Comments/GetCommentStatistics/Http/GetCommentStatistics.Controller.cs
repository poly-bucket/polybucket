using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.GetCommentStatistics.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class GetCommentStatisticsController(IEnhancedCommentsPlugin commentsPlugin) : ControllerBase
{
    /// <summary>
    /// Gets comment totals and top commenters for a target.
    /// </summary>
    /// <param name="targetType">model, user, userprofile, collection, or report.</param>
    /// <param name="targetId">The target's id.</param>
    /// <response code="200">The statistics.</response>
    /// <response code="400">The target type is invalid.</response>
    /// <response code="401">The caller is not signed in.</response>
    [HttpGet("statistics/{targetType}/{targetId:guid}")]
    [ProducesResponseType(typeof(CommentStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatistics(string targetType, Guid targetId)
    {
        if (!CommentTargetParser.TryParse(targetType, targetId, out var target))
        {
            return BadRequest(new { message = $"Invalid target type: {targetType}" });
        }

        return Ok(await commentsPlugin.GetCommentStatisticsAsync(target));
    }
}
