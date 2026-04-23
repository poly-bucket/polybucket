using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.DownloadModel.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DownloadModel.Http;

[ApiController]
[Route("api/models")]
public class DownloadModelController : ControllerBase
{
    private readonly IDownloadModelService _downloadModelService;

    public DownloadModelController(IDownloadModelService downloadModelService)
    {
        _downloadModelService = downloadModelService;
    }

    /// <summary>
    /// Download a model as a single file or a ZIP of files and previews.
    /// </summary>
    [HttpGet("{id}/download")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODEL_DOWNLOAD)]
    public async Task<IActionResult> DownloadModel(Guid id, CancellationToken cancellationToken = default)
    {
        var outcome = await _downloadModelService.DownloadAsync(id, User, cancellationToken);
        return outcome.Kind switch
        {
            DownloadModelOutcomeKind.NotFound => NotFound(outcome.Message),
            DownloadModelOutcomeKind.Forbid => StatusCode(403, outcome.Message),
            DownloadModelOutcomeKind.Error => StatusCode(500, outcome.Message),
            DownloadModelOutcomeKind.OkSingleFile when outcome.FileStream != null && outcome.FileName != null && outcome.FileContentType != null
                => File(outcome.FileStream, outcome.FileContentType, outcome.FileName),
            DownloadModelOutcomeKind.OkZip when outcome.ZipBytes != null && outcome.ZipFileName != null
                => File(outcome.ZipBytes, "application/zip", outcome.ZipFileName),
            _ => StatusCode(500, "An error occurred while downloading the model")
        };
    }
}
