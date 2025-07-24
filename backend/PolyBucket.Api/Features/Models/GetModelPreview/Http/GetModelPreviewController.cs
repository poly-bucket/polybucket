using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GetModelPreview.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelPreview.Http
{
    [ApiController]
    [Route("api/models")]
    public class GetModelPreviewController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Gets a preview image for a specific model and size
        /// </summary>
        /// <param name="modelId">The ID of the model</param>
        /// <param name="size">The size of the preview (thumbnail, medium, large)</param>
        /// <returns>Preview information including URL and status</returns>
        [HttpGet("{modelId}/previews/{size}")]
        [ProducesResponseType(typeof(GetModelPreviewResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GetModelPreviewResponse>> GetModelPreview(Guid modelId, string size)
        {
            var query = new GetModelPreviewQuery
            {
                ModelId = modelId,
                Size = size
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
} 