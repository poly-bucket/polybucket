using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class GenerateCustomThumbnailController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Generates a custom thumbnail for a model with user-defined settings
        /// </summary>
        /// <param name="modelId">The ID of the model</param>
        /// <param name="request">Custom thumbnail generation settings</param>
        /// <returns>Generation status and queue information</returns>
        [HttpPost("{modelId}/custom-thumbnail")]
        [ProducesResponseType(typeof(GenerateCustomThumbnailResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GenerateCustomThumbnailResponse>> GenerateCustomThumbnail(
            Guid modelId,
            [FromBody] GenerateCustomThumbnailRequest request)
        {
            if (request.Settings == null)
            {
                return BadRequest("Settings are required");
            }

            var command = new GenerateCustomThumbnailCommand
            {
                ModelId = modelId,
                ModelFileUrl = request.ModelFileUrl,
                FileType = request.FileType,
                Size = request.Size ?? "thumbnail",
                Settings = request.Settings,
                ForceRegenerate = request.ForceRegenerate
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }

    public class GenerateCustomThumbnailRequest
    {
        public string ModelFileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? Size { get; set; }
        public PreviewGenerationSettings? Settings { get; set; }
        public bool ForceRegenerate { get; set; } = false;
    }
} 