using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Plugins.Http;

[ApiController]
[Route("api/admin/marketplace")]
[Authorize]
public class MarketplaceSettingsController(PolyBucketDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Get the configured marketplace base URL.
    /// </summary>
    [HttpGet("base-url")]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(typeof(MarketplaceBaseUrlResponse), 200)]
    public async Task<ActionResult<MarketplaceBaseUrlResponse>> GetMarketplaceBaseUrl(CancellationToken cancellationToken)
    {
        var value = await dbContext.SystemSettings
            .AsNoTracking()
            .Where(s => s.Key == SystemSettingKeys.MarketplaceBaseUrl)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(new MarketplaceBaseUrlResponse { BaseUrl = value ?? string.Empty });
    }

    /// <summary>
    /// Save the marketplace base URL used for plugin catalog browsing and install.
    /// </summary>
    [HttpPut("base-url")]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(typeof(MarketplaceBaseUrlResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MarketplaceBaseUrlResponse>> UpdateMarketplaceBaseUrl(
        [FromBody] UpdateMarketplaceBaseUrlRequest request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out var uri))
        {
            return BadRequest(new { message = "Base URL must be a valid absolute URL" });
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Base URL must use http or https" });
        }

        var normalized = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        var entity = await dbContext.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.MarketplaceBaseUrl, cancellationToken);

        if (entity == null)
        {
            entity = new Database.Entities.SystemSettings.SystemSetting
            {
                Key = SystemSettingKeys.MarketplaceBaseUrl,
                Value = normalized
            };
            dbContext.SystemSettings.Add(entity);
        }
        else
        {
            entity.Value = normalized;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new MarketplaceBaseUrlResponse { BaseUrl = normalized });
    }
}

public class MarketplaceBaseUrlResponse
{
    public string BaseUrl { get; set; } = string.Empty;
}

public class UpdateMarketplaceBaseUrlRequest
{
    public string BaseUrl { get; set; } = string.Empty;
}
