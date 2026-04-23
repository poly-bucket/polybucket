using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.DownloadModel.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using ModelEntity = PolyBucket.Api.Common.Models.Model;

namespace PolyBucket.Api.Features.Models.DownloadModel.Repository;

public class DownloadModelRepository(PolyBucketDbContext context) : IDownloadModelRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<DownloadModelBundle?> GetBundleForDownloadAsync(
        Guid modelId,
        CancellationToken cancellationToken = default)
    {
        var model = await _context.Set<ModelEntity>()
            .AsNoTracking()
            .Include(m => m.Files)
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);

        if (model == null)
        {
            return null;
        }

        var previews = await _context.Set<ModelPreview>()
            .AsNoTracking()
            .Where(p => p.ModelId == modelId && p.Status == PreviewStatus.Completed)
            .ToListAsync(cancellationToken);

        return new DownloadModelBundle
        {
            Id = model.Id,
            Name = model.Name,
            ThumbnailUrl = model.ThumbnailUrl,
            Privacy = model.Privacy,
            AuthorId = model.AuthorId,
            Downloads = model.Downloads,
            Files = model.Files
                .Select(f => new DownloadModelFileItem
                {
                    Id = f.Id,
                    Name = f.Name,
                    Path = f.Path,
                    MimeType = f.MimeType
                })
                .ToList(),
            Previews = previews
                .Select(p => new DownloadModelPreviewItem
                {
                    Id = p.Id,
                    Size = p.Size,
                    StorageKey = p.StorageKey,
                    Width = p.Width,
                    Height = p.Height
                })
                .ToList()
        };
    }

    public async Task<bool> TryIncrementDownloadCountAsync(
        Guid modelId,
        CancellationToken cancellationToken = default)
    {
        var model = await _context.Set<ModelEntity>()
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);
        if (model == null)
        {
            return false;
        }

        model.Downloads++;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
