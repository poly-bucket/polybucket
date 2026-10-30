namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Domain;

public interface IUpdateSiteModelSettingsService
{
    Task<UpdateSiteModelSettingsResponse> UpdateSiteModelSettingsAsync(UpdateSiteModelSettingsCommand request, CancellationToken cancellationToken = default);
}
