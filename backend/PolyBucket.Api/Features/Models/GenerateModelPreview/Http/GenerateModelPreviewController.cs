using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class GenerateModelPreviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GenerateModelPreviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Triggers generation of a preview image for a specific model and size
        /// </summary>
        /// <param name="modelId">The ID of the model</param>
        /// <param name="size">The size of the preview (thumbnail, medium, large)</param>
        /// <param name="forceRegenerate">Whether to force regeneration if preview already exists</param>
        /// <returns>Generation status and queue information</returns>
        [HttpPost("{modelId}/previews")]
        [ProducesResponseType(typeof(GenerateModelPreviewResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GenerateModelPreviewResponse>> GenerateModelPreview(
            Guid modelId, 
            [FromQuery] string size = "thumbnail",
            [FromQuery] bool forceRegenerate = false)
        {
            var command = new GenerateModelPreviewCommand
            {
                ModelId = modelId,
                Size = size,
                ForceRegenerate = forceRegenerate
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
} 