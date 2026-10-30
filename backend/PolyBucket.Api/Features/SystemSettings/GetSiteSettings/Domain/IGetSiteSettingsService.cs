namespace PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Domain;

public interface IGetSiteSettingsService
{
    Task<GetSiteSettingsResponse> GetSiteSettingsAsync(GetSiteSettingsQuery request, CancellationToken cancellationToken = default);
}
