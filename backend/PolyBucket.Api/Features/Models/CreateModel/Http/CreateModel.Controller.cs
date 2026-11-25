using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModel.Http
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/models")]
    [ServiceFilter(typeof(RequireModelUploadAuthorizationFilter))]
    public class CreateModelController : ControllerBase
    {
        private readonly CreateModelService _createModelService;
        private readonly ILogger<CreateModelController> _logger;

        public CreateModelController(CreateModelService createModelService, ILogger<CreateModelController> logger)
        {
            _createModelService = createModelService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new model with file uploads.
        /// </summary>
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
            [FromForm] string? remixUrl,
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
                    Remix = remix,
                    RemixUrl = remixUrl
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