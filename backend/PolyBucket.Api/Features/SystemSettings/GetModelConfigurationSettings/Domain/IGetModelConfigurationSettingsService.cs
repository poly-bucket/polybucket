namespace PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Domain;

public interface IGetModelConfigurationSettingsService
{
    Task<GetModelConfigurationSettingsResponse> GetModelConfigurationSettingsAsync(GetModelConfigurationSettingsQuery request, CancellationToken cancellationToken = default);
}
