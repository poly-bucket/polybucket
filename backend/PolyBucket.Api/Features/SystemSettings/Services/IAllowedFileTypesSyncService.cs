namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface IAllowedFileTypesSyncService
{
    Task<string> SyncFromFileTypeSettingsAsync(CancellationToken cancellationToken = default);

    Task<string> GetDerivedAllowedFileTypesAsync(CancellationToken cancellationToken = default);
}
