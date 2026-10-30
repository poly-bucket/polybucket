using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.Authorization;
using PolyBucket.Api.Features.Comments.CreateComment.Domain;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.CreateComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CreateCommentController(ICreateCommentService createCommentService) : ControllerBase
{
    /// <summary>
    /// Adds a comment, or a reply when a parent comment is given.
    /// </summary>
    /// <remarks>Replies to a reply are attached to the top-level comment so threads stay one level deep.</remarks>
    /// <param name="request">The target, content, and optional parent comment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The created comment.</response>
    /// <response code="400">The comment could not be added.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The user's email address must be verified first.</response>
    [HttpPost]
    [RequireVerifiedEmail]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetCommentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await createCommentService.CreateAsync(userId.Value, request.Target, request.Content, request.ParentCommentId, User.IsCommentAdmin(), cancellationToken);
        return result.ErrorMessage == null
            ? Ok(result.Comment)
            : BadRequest(new { message = result.ErrorMessage });
    }
}
