namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

public interface IGetFileSettingsService
{
    Task<GetFileSettingsResponse> GetFileSettingsAsync(CancellationToken cancellationToken = default);
}
