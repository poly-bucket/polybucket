using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using ApiGetStorageSettings = PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Domain;
using PolyBucket.Storage;
using PolyBucket.Storage.GetStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;

[Authorize]
[ApiController]
[Route("api/system-settings/storage")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class GetStorageSettingsController(ApiGetStorageSettings.IGetStorageSettingsService getStorageSettingsService) : ControllerBase
{
    /// <summary>
    /// Gets the effective object storage configuration stored in the database.
    /// </summary>
    /// <remarks>The secret key is never returned; <c>hasSecretKey</c> indicates whether one is stored.</remarks>
    /// <response code="200">The effective storage settings.</response>
    [HttpGet]
    [ProducesResponseType(typeof(StorageSettingsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<StorageSettingsDto>> GetStorageSettings(CancellationToken cancellationToken = default)
    {
        return Ok(await getStorageSettingsService.GetStorageSettingsAsync(cancellationToken));
    }
}
