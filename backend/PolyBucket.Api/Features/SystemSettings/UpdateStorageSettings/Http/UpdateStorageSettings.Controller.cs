using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Storage;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Http;

[Authorize]
[ApiController]
[Route("api/system-settings/storage")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class UpdateStorageSettingsController(IUpdateStorageSettingsService service) : ControllerBase
{
    /// <summary>
    /// Updates the object storage configuration stored in the database.
    /// </summary>
    /// <remarks>
    /// Leave <c>secretKey</c> empty to keep the stored secret, or set <c>clearSecretKey</c> to remove it.
    /// </remarks>
    /// <response code="200">The updated effective storage settings.</response>
    /// <response code="400">The settings are invalid.</response>
    [HttpPut]
    [ProducesResponseType(typeof(StorageSettingsDto), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<StorageSettingsDto>> UpdateStorageSettings(
        [FromBody] GetStorageSettings.Http.UpdateStorageSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        try
        {
            var result = await service.UpdateAsync(
                new StorageSettingsUpdate
                {
                    Provider = request.Provider,
                    Endpoint = request.Endpoint,
                    Port = request.Port,
                    UseSsl = request.UseSsl,
                    BucketName = request.BucketName,
                    AccessKey = request.AccessKey,
                    SecretKey = request.SecretKey,
                    ClearSecretKey = request.ClearSecretKey,
                    ExternalEndpoint = request.ExternalEndpoint,
                    ExternalPort = request.ExternalPort,
                    ExternalUseSsl = request.ExternalUseSsl,
                    Region = request.Region,
                    ForcePathStyle = request.ForcePathStyle
                },
                cancellationToken);

            return Ok(result);
        }
        catch (StorageSettingsValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return BadRequest(new ValidationProblemDetails(ModelState));
        }
    }
}
