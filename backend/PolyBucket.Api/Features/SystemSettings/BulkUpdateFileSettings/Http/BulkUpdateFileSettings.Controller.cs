using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Http;

[ApiController]
[Route("api/system-settings/file-settings")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class BulkUpdateFileSettingsController(IBulkUpdateFileSettingsService bulkUpdateFileSettingsService) : ControllerBase
{
    private readonly IBulkUpdateFileSettingsService _bulkUpdateFileSettingsService = bulkUpdateFileSettingsService;

    /// <summary>
    /// Bulk enable or disable file type settings by ID
    /// </summary>
    /// <param name="command">File type IDs and target enabled state</param>
    /// <returns>Per-item update results</returns>
    [HttpPost("bulk-update")]
    [ProducesResponseType(200, Type = typeof(BulkUpdateFileSettingsResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<BulkUpdateFileSettingsResponse>> BulkUpdateFileSettings(
        [FromBody] BulkUpdateFileSettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Ids == null || command.Ids.Count == 0)
        {
            return BadRequest("At least one file type id is required");
        }

        var result = await _bulkUpdateFileSettingsService.BulkUpdateAsync(command, cancellationToken);
        return Ok(result);
    }
}
