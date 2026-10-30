namespace PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Domain;

public interface IUpdateFileSettingsService
{
    Task<UpdateFileSettingsResponse> UpdateFileSettingsAsync(UpdateFileSettingsCommand command, CancellationToken cancellationToken = default);
}
