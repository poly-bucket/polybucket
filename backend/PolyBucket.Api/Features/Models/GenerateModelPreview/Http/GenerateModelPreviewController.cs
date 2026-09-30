using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class GenerateModelPreviewController(IGenerateModelPreviewService generateModelPreviewService) : ControllerBase
    {
        private readonly IGenerateModelPreviewService _generateModelPreviewService = generateModelPreviewService;

        /// <summary>
        /// Queues generation of a preview image for a specific model and size.
        /// </summary>
        /// <remarks>
        /// Rendering happens in a background worker. A completed preview is only re-rendered when
        /// <paramref name="forceRegenerate"/> is set; a failed preview is always re-queued.
        /// Only the model's owner or users with the edit-any-model permission can request previews.
        /// </remarks>
        /// <param name="modelId">The ID of the model</param>
        /// <param name="size">The size of the preview (thumbnail, medium, large)</param>
        /// <param name="forceRegenerate">Whether to force regeneration if preview already exists</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <response code="200">The preview's queue status.</response>
        /// <response code="400">The size is not supported.</response>
        /// <response code="401">The caller is not signed in.</response>
        /// <response code="403">The caller cannot manage this model.</response>
        /// <response code="404">The model does not exist.</response>
        [HttpPost("{modelId}/previews")]
        [ProducesResponseType(typeof(GenerateModelPreviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenerateModelPreviewResponse>> GenerateModelPreview(
            Guid modelId,
            [FromQuery] string size = "thumbnail",
            [FromQuery] bool forceRegenerate = false,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(User.FindUserIdClaim(), out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var response = await _generateModelPreviewService.RequestPreviewAsync(modelId, size, forceRegenerate, userId, cancellationToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
