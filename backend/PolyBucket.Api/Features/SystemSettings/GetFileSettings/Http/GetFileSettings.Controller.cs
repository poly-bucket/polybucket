using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Http;

[ApiController]
[Route("api/system-settings/file-settings")]
[Authorize]
public class GetFileSettingsController(IGetFileSettingsService getFileSettingsService) : ControllerBase
{
    private readonly IGetFileSettingsService _getFileSettingsService = getFileSettingsService;

    /// <summary>
    /// Get file type settings with pagination, search, and category filter
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search extension, display name, MIME type, or description</param>
    /// <param name="category">Filter by category; omit or use All for no filter</param>
    /// <returns>Paginated file type configuration</returns>
    [HttpGet]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(200, Type = typeof(GetFileSettingsResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<GetFileSettingsResponse>> GetFileSettings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return BadRequest("Page number must be greater than 0");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Page size must be between 1 and 100");
        }

        var query = new GetFileSettingsQueryParams
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            Category = category
        };

        var result = await _getFileSettingsService.GetFileSettingsAsync(query, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
