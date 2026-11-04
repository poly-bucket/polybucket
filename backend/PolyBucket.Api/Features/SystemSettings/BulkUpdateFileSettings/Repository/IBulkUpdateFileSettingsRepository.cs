namespace PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Repository;

public interface IBulkUpdateFileSettingsRepository
{
    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<PolyBucket.Database.Entities.SystemSettings.FileTypeSettings>> GetFileTypesByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken);
}
