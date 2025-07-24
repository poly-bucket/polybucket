using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Comments.Domain;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Comments.Queries
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class GetCommentsForModelController(ICommentsPlugin commentsPlugin) : ControllerBase
    {
        private readonly ICommentsPlugin _commentsPlugin = commentsPlugin;

        [HttpGet("model/{modelId}")]
        public async Task<IActionResult> GetCommentsForModel(Guid modelId)
        {
            var comments = await _commentsPlugin.GetCommentsForModelAsync(modelId);
            return Ok(comments);
        }
    }
} 