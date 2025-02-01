using Core.Plugins.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers.Comments
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentsPlugin _commentsPlugin;

        public CommentsController(ICommentsPlugin commentsPlugin)
        {
            _commentsPlugin = commentsPlugin;
        }

        [HttpGet("model/{modelId}")]
        public async Task<IActionResult> GetCommentsForModel(Guid modelId)
        {
            var comments = await _commentsPlugin.GetCommentsForModelAsync(modelId);
            return Ok(comments);
        }

        [HttpPost("model/{modelId}")]
        public async Task<IActionResult> AddComment(Guid modelId, [FromBody] AddCommentRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            var comment = await _commentsPlugin.AddCommentAsync(modelId, userId, request.Content);
            return Ok(comment);
        }

        [HttpPost("{commentId}/like")]
        public async Task<IActionResult> LikeComment(Guid commentId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            await _commentsPlugin.LikeCommentAsync(commentId, userId);
            return Ok();
        }

        [HttpPost("{commentId}/dislike")]
        public async Task<IActionResult> DislikeComment(Guid commentId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            await _commentsPlugin.DislikeCommentAsync(commentId, userId);
            return Ok();
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

    public class AddCommentRequest
    {
        public string Content { get; set; }
    }
} 