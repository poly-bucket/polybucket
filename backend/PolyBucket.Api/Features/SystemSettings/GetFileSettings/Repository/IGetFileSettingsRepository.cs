using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Repository;

public interface IGetFileSettingsRepository
{
    Task<IReadOnlyList<FileTypeSettingsData>> GetFileTypesAsync(CancellationToken cancellationToken);
}
