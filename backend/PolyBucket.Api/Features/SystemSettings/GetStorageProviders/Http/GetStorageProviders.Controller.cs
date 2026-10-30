using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Domain;
using PolyBucket.Storage;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Http;

[Authorize]
[ApiController]
[Route("api/system-settings/storage")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class GetStorageProvidersController(IGetStorageProvidersService getStorageProvidersService) : ControllerBase
{
    /// <summary>
    /// Lists supported object storage providers and default connection values for the setup wizard.
    /// </summary>
    /// <response code="200">Supported storage providers and defaults.</response>
    [HttpGet("providers")]
    [ProducesResponseType(typeof(StorageProvidersResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public ActionResult<StorageProvidersResponseDto> GetStorageProviders()
    {
        return Ok(getStorageProvidersService.GetStorageProviders());
    }
}
