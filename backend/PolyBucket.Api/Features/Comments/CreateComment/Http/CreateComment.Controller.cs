using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.Authorization;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.CreateComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CreateCommentController(IEnhancedCommentsPlugin commentsPlugin, ICommentResponseMapper mapper) : ControllerBase
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

        try
        {
            var comment = await commentsPlugin.AddCommentAsync(request.Target, userId.Value, request.Content, request.ParentCommentId);
            var mapped = await mapper.MapAsync(new[] { comment }, userId, User.IsCommentAdmin(), includeReplies: false, cancellationToken);
            return Ok(mapped[0]);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
