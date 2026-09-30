using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.Authorization;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;

namespace PolyBucket.Api.Features.Comments.UpdateComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class UpdateCommentController(IEnhancedCommentsPlugin commentsPlugin, ICommentResponseMapper mapper) : ControllerBase
{
    /// <summary>
    /// Edits the content of the caller's own comment.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="request">The new content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The updated comment.</response>
    /// <response code="400">The comment could not be updated.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The caller does not own the comment, or their email must be verified first.</response>
    [HttpPut("{commentId:guid}")]
    [RequireVerifiedEmail]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentContentRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetCommentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var comment = await commentsPlugin.UpdateCommentAsync(commentId, userId.Value, request.Content);
            var mapped = await mapper.MapAsync(new[] { comment }, userId, User.IsCommentAdmin(), includeReplies: true, cancellationToken);
            return Ok(mapped[0]);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
