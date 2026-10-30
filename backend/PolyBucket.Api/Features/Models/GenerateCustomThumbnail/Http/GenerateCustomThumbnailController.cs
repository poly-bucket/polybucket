using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;

namespace PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Http;

[Authorize]
[ApiController]
[Route("api/models")]
public class GenerateCustomThumbnailController(IGenerateCustomThumbnailService service) : ControllerBase
{
    /// <summary>
    /// Generates a custom thumbnail for a model with user-defined settings.
    /// </summary>
    [HttpPost("{modelId:guid}/custom-thumbnail")]
    [ProducesResponseType(typeof(GenerateCustomThumbnailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenerateCustomThumbnailResponse>> GenerateCustomThumbnail(
        Guid modelId,
        [FromBody] GenerateCustomThumbnailRequest request,
        CancellationToken cancellationToken)
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

        return Ok(await service.GenerateAsync(command, cancellationToken));
    }
}

public class GenerateCustomThumbnailRequest
{
    public string ModelFileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string? Size { get; set; }
    public PreviewGenerationSettings? Settings { get; set; }
    public bool ForceRegenerate { get; set; }
}
