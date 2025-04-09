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
    public class AddCommentController : ControllerBase
    {
        private readonly ICommentsPlugin _commentsPlugin;

        public AddCommentController(ICommentsPlugin commentsPlugin)
        {
            _commentsPlugin = commentsPlugin;
        }

        [HttpPost("model/{modelId}")]
        public async Task<IActionResult> AddComment(Guid modelId, [FromBody] AddCommentRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            var comment = await _commentsPlugin.AddCommentAsync(modelId, userId, request.Content);
            return Ok(comment);
        }
    }
} 