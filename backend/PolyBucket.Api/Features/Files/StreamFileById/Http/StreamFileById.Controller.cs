using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Files.StreamFileById.Domain;

namespace PolyBucket.Api.Features.Files.StreamFileById.Http;

[ApiController]
[Route("api/files")]
public class StreamFileByIdController(IStreamFileByIdService service) : ControllerBase
{
    /// <summary>
    /// Stream a model file by file identifier when the caller has download permission.
    /// </summary>
    [HttpGet("stream/{fileId:guid}")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODEL_DOWNLOAD)]
    [Produces("application/octet-stream", "application/json")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StreamFile(Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.StreamAsync(fileId, User, cancellationToken);
            return result.Status switch
            {
                StreamFileByIdStatus.NotFound => NotFound("File not found"),
                StreamFileByIdStatus.Forbidden => Forbid("You do not have permission to access this file"),
                StreamFileByIdStatus.StorageMissing => NotFound("File not found in storage"),
                StreamFileByIdStatus.Success when result.Content != null => BuildFileResult(result),
                _ => StatusCode(500, "An error occurred while streaming the file")
            };
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while streaming the file");
        }
    }

    private FileStreamResult BuildFileResult(StreamFileByIdResult result)
    {
        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{result.FileName}\"");
        Response.Headers.Append("Cache-Control", "public, max-age=3600");
        return File(result.Content!, result.MimeType ?? "application/octet-stream");
    }
}
