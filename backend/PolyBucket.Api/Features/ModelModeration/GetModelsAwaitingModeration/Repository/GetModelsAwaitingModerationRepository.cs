using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Repository;

public class GetModelsAwaitingModerationRepository(PolyBucketDbContext context) : IGetModelsAwaitingModerationRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<ModelsAwaitingModerationResponse> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var pendingModelIds = _context.ModelModerationRecords
            .AsNoTracking()
            .Where(r => r.Status == ModelModerationStatus.Pending)
            .Select(r => r.ModelId);

        var query = _context.Models
            .AsNoTracking()
            .Where(m => m.DeletedAt == null && pendingModelIds.Contains(m.Id))
            .Include(m => m.Author)
            .Include(m => m.Files)
            .OrderBy(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;

        var models = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var recordStatuses = await _context.ModelModerationRecords
            .AsNoTracking()
            .Where(r => models.Select(m => m.Id).Contains(r.ModelId))
            .ToDictionaryAsync(r => r.ModelId, r => r.Status, cancellationToken);

        var items = models.Select(model =>
        {
            var firstFile = model.Files.OrderBy(f => f.CreatedAt).FirstOrDefault();
            var fileFormat = firstFile != null ? Path.GetExtension(firstFile.Name).TrimStart('.') : string.Empty;

            return new ModelModerationQueueItem
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                ThumbnailUrl = model.ThumbnailUrl,
                UserName = model.Author?.Username ?? string.Empty,
                FileFormat = fileFormat,
                CreatedAt = model.CreatedAt,
                Status = recordStatuses.GetValueOrDefault(model.Id, ModelModerationStatus.Pending)
            };
        }).ToList();

        return new ModelsAwaitingModerationResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
