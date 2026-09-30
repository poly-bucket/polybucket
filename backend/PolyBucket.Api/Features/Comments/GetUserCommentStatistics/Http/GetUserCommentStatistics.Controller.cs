using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.GetUserCommentStatistics.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class GetUserCommentStatisticsController(IEnhancedCommentsPlugin commentsPlugin) : ControllerBase
{
    /// <summary>
    /// Gets comment totals for a user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <response code="200">The statistics.</response>
    /// <response code="401">The caller is not signed in.</response>
    [HttpGet("user/{userId:guid}/statistics")]
    [ProducesResponseType(typeof(UserCommentStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserStatistics(Guid userId)
    {
        return Ok(await commentsPlugin.GetUserCommentStatisticsAsync(userId));
    }
}
