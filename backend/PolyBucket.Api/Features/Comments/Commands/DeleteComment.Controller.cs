using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.Commands
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class DeleteCommentController : ControllerBase
    {
        private readonly ICommentsPlugin _commentsPlugin;

        public DeleteCommentController(ICommentsPlugin commentsPlugin)
        {
            _commentsPlugin = commentsPlugin;
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            await _commentsPlugin.DeleteCommentAsync(commentId, userId);
            return Ok();
        }
    }
} 