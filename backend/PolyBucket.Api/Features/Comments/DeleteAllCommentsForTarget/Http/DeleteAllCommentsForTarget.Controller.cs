using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Http;

[ApiController]
[Route("api/comments")]
[Authorize(Roles = "Admin")]
public class DeleteAllCommentsForTargetController(IDeleteAllCommentsForTargetService deleteAllCommentsForTargetService) : ControllerBase
{
    /// <summary>
    /// Deletes every comment on a target.
    /// </summary>
    /// <param name="targetType">model, user, userprofile, collection, or report.</param>
    /// <param name="targetId">The target's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The comments were deleted.</response>
    /// <response code="400">The target type is invalid or the comments could not be deleted.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The caller is not an admin.</response>
    [HttpDelete("target/{targetType}/{targetId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAllCommentsForTarget(string targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var userId = User.GetCommentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await deleteAllCommentsForTargetService.DeleteAllAsync(targetType, targetId, userId.Value, cancellationToken);
        return result.Success
            ? Ok(new { message = "All comments deleted successfully" })
            : BadRequest(new { message = result.ErrorMessage });
    }
}
