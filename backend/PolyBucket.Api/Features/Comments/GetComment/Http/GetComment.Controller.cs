using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.GetComment.Domain;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.Plugins;

namespace PolyBucket.Api.Features.Comments.GetComment.Http;

[ApiController]
[Route("api/comments")]
[Authorize]
public class GetCommentController(IGetCommentService getCommentService) : ControllerBase
{
    /// <summary>
    /// Gets a single comment with its replies.
    /// </summary>
    /// <param name="commentId">The comment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The comment.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="404">The comment does not exist or is hidden by moderation.</response>
    [HttpGet("{commentId:guid}")]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComment(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await getCommentService.GetAsync(commentId, User.GetCommentUserId(), User.IsCommentAdmin(), User.IsCommentModerator(), cancellationToken);
        return result.NotFound ? NotFound() : Ok(result.Comment);
    }
}
