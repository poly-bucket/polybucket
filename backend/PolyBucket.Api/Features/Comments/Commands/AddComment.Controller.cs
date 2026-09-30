using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Authentication.Authorization;
using PolyBucket.Api.Features.Comments.Domain;

namespace PolyBucket.Api.Features.Comments.Commands
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class AddCommentController(ICommentsPlugin commentsPlugin) : ControllerBase
    {
        private readonly ICommentsPlugin _commentsPlugin = commentsPlugin;

        /// <summary>
        /// Adds a comment to a model.
        /// </summary>
        /// <response code="200">The created comment.</response>
        /// <response code="403">The user's email address must be verified first.</response>
        [HttpPost("model/{modelId}")]
        [RequireVerifiedEmail]
        public async Task<IActionResult> AddComment(Guid modelId, [FromBody] AddCommentRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            var comment = await _commentsPlugin.AddCommentAsync(modelId, userId, request.Content);
            return Ok(comment);
        }
    }
} 