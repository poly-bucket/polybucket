using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Storage;
using PolyBucket.Storage.GetStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;

[Authorize]
[ApiController]
[Route("api/system-settings/storage")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class GetStorageSettingsController(IGetStorageSettingsService service) : ControllerBase
{
    /// <summary>
    /// Gets the effective object storage configuration, including where each value comes from and which fields are managed by environment variables.
    /// </summary>
    /// <remarks>The secret key is never returned; <c>hasSecretKey</c> indicates whether one is stored.</remarks>
    /// <response code="200">The effective storage settings.</response>
    [HttpGet]
    [ProducesResponseType(typeof(StorageSettingsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<StorageSettingsDto>> GetStorageSettings(CancellationToken cancellationToken = default)
    {
        return Ok(await service.GetAsync(cancellationToken));
    }
}
