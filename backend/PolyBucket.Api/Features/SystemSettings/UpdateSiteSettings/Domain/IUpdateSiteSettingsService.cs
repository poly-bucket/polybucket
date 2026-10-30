namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Domain;

public interface IUpdateSiteSettingsService
{
    Task<UpdateSiteSettingsResponse> UpdateSiteSettingsAsync(UpdateSiteSettingsCommand request, CancellationToken cancellationToken = default);
}
