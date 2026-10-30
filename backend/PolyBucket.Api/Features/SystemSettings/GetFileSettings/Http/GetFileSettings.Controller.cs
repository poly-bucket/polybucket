using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Http;

[ApiController]
[Route("api/system-settings/file-settings")]
public class GetFileSettingsController(IGetFileSettingsService getFileSettingsService) : ControllerBase
{
    private readonly IGetFileSettingsService _getFileSettingsService = getFileSettingsService;

    /// <summary>
    /// Get the current file type settings
    /// </summary>
    /// <returns>Current file type configuration</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(GetFileSettingsResponse))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<GetFileSettingsResponse>> GetFileSettings(CancellationToken cancellationToken)
    {
        var result = await _getFileSettingsService.GetFileSettingsAsync(cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
