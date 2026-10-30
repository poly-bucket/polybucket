using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Files.StreamModelFile.Domain;

namespace PolyBucket.Api.Features.Files.StreamModelFile.Http;

[ApiController]
[Route("api/files")]
public class StreamModelFileController(IStreamModelFileService service) : ControllerBase
{
    /// <summary>
    /// Stream a named file from a model; optionally records a download when requested.
    /// </summary>
    [HttpGet("stream/model/{modelId:guid}/{fileName}")]
    [ServiceFilter(typeof(PublicModelAuthorizationFilter))]
    [Produces("application/octet-stream", "application/json")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StreamModelFile(
        Guid modelId,
        string fileName,
        [FromQuery] bool recordDownload = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.StreamAsync(modelId, fileName, recordDownload, HttpContext, User, cancellationToken);
            return result.Status switch
            {
                StreamModelFileStatus.ModelNotFound => NotFound("Model not found"),
                StreamModelFileStatus.Forbidden => Forbid("You do not have permission to access this model"),
                StreamModelFileStatus.FileNotFound => NotFound($"File '{Uri.UnescapeDataString(fileName)}' not found. Available files: {result.AvailableFilesMessage}"),
                StreamModelFileStatus.InvalidPath => StatusCode(500, "Invalid file path format"),
                StreamModelFileStatus.StorageMissing => NotFound("File not found in storage"),
                StreamModelFileStatus.Success when result.Content != null => BuildFileResult(result),
                _ => StatusCode(500, "An error occurred while streaming the file")
            };
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while streaming the file");
        }
    }

    private FileStreamResult BuildFileResult(StreamModelFileResult result)
    {
        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{result.FileName}\"");
        Response.Headers.Append("Cache-Control", "public, max-age=3600");
        if (result.DownloadCounted || result.Downloads > 0)
        {
            Response.ApplyModelDownloadCountHeaders(result.Downloads, result.DownloadCounted);
        }

        return File(result.Content!, result.MimeType ?? "application/octet-stream");
    }
}
