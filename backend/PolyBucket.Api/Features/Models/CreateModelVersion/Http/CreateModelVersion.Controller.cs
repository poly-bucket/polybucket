using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using System;
using System.Threading.Tasks;
using System.Threading;


namespace PolyBucket.Api.Features.Models.CreateModelVersion.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    [RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_EDIT_OWN, PermissionConstants.MODEL_EDIT_ANY)]
    public class CreateModelVersionController(ICreateModelVersionService createModelVersionService, ILogger<CreateModelVersionController> logger) : ControllerBase
    {
        private readonly ICreateModelVersionService _createModelVersionService = createModelVersionService;
        private readonly ILogger<CreateModelVersionController> _logger = logger;

        [HttpPost("{id}/versions")]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CreateModelVersionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CreateModelVersionResponse>> CreateModelVersion(
            Guid id,
            [FromForm] string name,
            [FromForm] string? notes,
            [FromForm] string? thumbnailFileId,
            [FromForm] IFormFile[] files,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new CreateModelVersionRequest
                {
                    Name = name,
                    Notes = notes,
                    ThumbnailFileId = thumbnailFileId,
                    Files = files
                };
                var response = await _createModelVersionService.CreateModelVersionAsync(id, request, User, cancellationToken);
                return Created($"/api/models/{id}/versions/{response.ModelVersion.Id}", response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error creating model version for model {ModelId}: {Message}", id, ex.Message);
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
                _logger.LogError(ex, "Failed to create model version for model {ModelId}", id);
                return StatusCode(500, "An error occurred while creating the model version");
            }
        }
    }
} 