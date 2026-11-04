using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.SystemSettings.SyncAllowedFileTypes.Domain;

namespace PolyBucket.Api.Features.SystemSettings.SyncAllowedFileTypes.Http;

[ApiController]
[Route("api/system-settings/file-settings")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class SyncAllowedFileTypesController(ISyncAllowedFileTypesService syncAllowedFileTypesService) : ControllerBase
{
    private readonly ISyncAllowedFileTypesService _syncAllowedFileTypesService = syncAllowedFileTypesService;

    /// <summary>
    /// Sync site AllowedFileTypes from enabled file type settings
    /// </summary>
    /// <returns>Derived comma-separated extension list</returns>
    [HttpPost("sync-allowed-types")]
    [ProducesResponseType(200, Type = typeof(SyncAllowedFileTypesResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<SyncAllowedFileTypesResponse>> SyncAllowedFileTypes(CancellationToken cancellationToken)
    {
        var result = await _syncAllowedFileTypesService.SyncAsync(cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
