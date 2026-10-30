using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using PolyBucket.Storage;

namespace PolyBucket.Api.Features.Files.StreamFileById.Repository;

public class StreamFileByIdRepository(PolyBucketDbContext context, IStorageService storageService) : IStreamFileByIdRepository
{
    public Task<ModelFile?> GetFileWithModelAsync(Guid fileId, CancellationToken cancellationToken = default) =>
        context.ModelFiles
            .Include(f => f.Model)
            .ThenInclude(m => m.Author)
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);

    public Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default) =>
        storageService.DownloadAsync(path);
}
