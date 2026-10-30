using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.ExportAccount.Repository;

public interface IExportAccountRepository
{
    Task<User?> GetUserForExportAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> GetModelCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> GetCollectionCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> GetFavoriteCollectionCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExportAccountModelSummary>> GetRecentModelsAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExportAccountCollectionSummary>> GetRecentCollectionsAsync(Guid userId, CancellationToken cancellationToken);
}

public class ExportAccountModelSummary
{
    public Guid ModelId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string Privacy { get; init; } = string.Empty;
}

public class ExportAccountCollectionSummary
{
    public Guid CollectionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Visibility { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
