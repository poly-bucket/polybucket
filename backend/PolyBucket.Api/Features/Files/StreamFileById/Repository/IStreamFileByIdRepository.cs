using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Files.StreamFileById.Repository;

public interface IStreamFileByIdRepository
{
    Task<ModelFile?> GetFileWithModelAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default);
}
