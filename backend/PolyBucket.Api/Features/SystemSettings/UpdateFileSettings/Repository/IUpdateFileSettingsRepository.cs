using PolyBucket.Database.Entities.SystemSettings;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Repository;

public interface IUpdateFileSettingsRepository
{
    Task<FileTypeSettings?> GetFileTypeAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
