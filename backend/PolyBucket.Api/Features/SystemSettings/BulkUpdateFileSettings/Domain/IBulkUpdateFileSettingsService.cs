namespace PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Domain;

public interface IBulkUpdateFileSettingsService
{
    Task<BulkUpdateFileSettingsResponse> BulkUpdateAsync(
        BulkUpdateFileSettingsCommand command,
        CancellationToken cancellationToken = default);
}
