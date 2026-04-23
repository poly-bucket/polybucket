using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModel.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    [RequirePermission(PermissionConstants.MODEL_CREATE)]
    public class CreateModelController : ControllerBase
    {
        private readonly CreateModelService _createModelService;
        private readonly ILogger<CreateModelController> _logger;

        public CreateModelController(CreateModelService createModelService, ILogger<CreateModelController> logger)
        {
            _createModelService = createModelService;
            _logger = logger;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CreateModelResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CreateModelResponse>> CreateModel(
            [FromForm] string name,
            [FromForm] string? description,
            [FromForm] IFormFile[] files,
            [FromForm] string? thumbnailFileId,
            [FromForm] string? privacy,
            [FromForm] string? license,
            [FromForm] bool aiGenerated,
            [FromForm] bool workInProgress,
            [FromForm] bool nSFW,
            [FromForm] bool remix,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new CreateModelRequest
                {
                    Name = name,
                    Description = description,
                    Files = files,
                    ThumbnailFileId = thumbnailFileId,
                    Privacy = privacy,
                    License = license,
                    AIGenerated = aiGenerated,
                    WorkInProgress = workInProgress,
                    NSFW = nSFW,
                    Remix = remix
                };
                var response = await _createModelService.CreateModelAsync(request, User, cancellationToken);
                return CreatedAtAction(nameof(CreateModel), new { id = response.Model.Id }, response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error during model creation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during model creation");
                return StatusCode(500, "An error occurred while creating the model");
            }
        }
    }
} 