using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.DeleteModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.LikeModel.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class LikeModelController(ILikeModelService likeModelService, ILogger<LikeModelController> logger) : ControllerBase
    {
        [HttpPost("{id}/like")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> LikeModel(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await likeModelService.LikeModelAsync(id, User, cancellationToken);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                logger.LogWarning("Validation error liking model {ModelId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ModelNotFoundException)
            {
                return NotFound("Model not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to like model {ModelId}", id);
                return StatusCode(500, "An error occurred while liking the model");
            }
        }

        [HttpDelete("{id}/like")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UnlikeModel(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await likeModelService.UnlikeModelAsync(id, User, cancellationToken);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                logger.LogWarning("Validation error unliking model {ModelId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ModelNotFoundException)
            {
                return NotFound("Model not found");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to unlike model {ModelId}", id);
                return StatusCode(500, "An error occurred while unliking the model");
            }
        }
    }
}
