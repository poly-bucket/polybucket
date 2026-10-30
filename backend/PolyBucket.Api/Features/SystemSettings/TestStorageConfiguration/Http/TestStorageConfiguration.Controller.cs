using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;
using ApiTestStorageConfiguration = PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Domain;
using PolyBucket.Storage.TestStorageConfiguration;
using PolyBucket.Storage.UpdateStorageSettings;

namespace PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Http;

[Authorize]
[ApiController]
[Route("api/system-settings/storage")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class TestStorageConfigurationController(ApiTestStorageConfiguration.ITestStorageConfigurationService testStorageConfigurationService) : ControllerBase
{
    /// <summary>
    /// Tests connectivity to object storage using saved settings or an optional proposed configuration.
    /// </summary>
    /// <response code="200">The connectivity test result.</response>
    [HttpPost("test")]
    [ProducesResponseType(typeof(TestStorageConfigurationResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<TestStorageConfigurationResult>> TestStorageConfiguration(
        [FromBody] UpdateStorageSettingsRequest? request,
        CancellationToken cancellationToken = default)
    {
        StorageSettingsUpdate? proposed = null;
        if (request != null)
        {
            proposed = new StorageSettingsUpdate
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
            };
        }

        return Ok(await testStorageConfigurationService.TestStorageConfigurationAsync(proposed, cancellationToken));
    }
}
