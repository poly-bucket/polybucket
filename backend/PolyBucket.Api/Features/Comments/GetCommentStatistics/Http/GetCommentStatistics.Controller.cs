using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.GetCommentStatistics.Domain;
using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.GetCommentStatistics.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class GetCommentStatisticsController(IGetCommentStatisticsService getCommentStatisticsService) : ControllerBase
{
    /// <summary>
    /// Gets comment totals and top commenters for a target.
    /// </summary>
    /// <param name="targetType">model, user, userprofile, collection, or report.</param>
    /// <param name="targetId">The target's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The statistics.</response>
    /// <response code="400">The target type is invalid.</response>
    /// <response code="401">The caller is not signed in.</response>
    [HttpGet("statistics/{targetType}/{targetId:guid}")]
    [ProducesResponseType(typeof(CommentStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatistics(string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var result = await getCommentStatisticsService.GetAsync(targetType, targetId, cancellationToken);
        return result.ErrorMessage == null
            ? Ok(result.Statistics)
            : BadRequest(new { message = result.ErrorMessage });
    }
}
