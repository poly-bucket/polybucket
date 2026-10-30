namespace PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain;

public interface IGetSiteModelSettingsService
{
    Task<GetSiteModelSettingsResponse> GetSiteModelSettingsAsync(GetSiteModelSettingsQuery request, CancellationToken cancellationToken = default);
}
