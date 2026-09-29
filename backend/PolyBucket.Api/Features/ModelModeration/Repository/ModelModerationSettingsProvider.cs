using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Repository;

public class ModelModerationSettingsProvider(PolyBucketDbContext context) : IModelModerationSettingsProvider
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<ModelModerationSettingsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var systemSetup = await _context.SystemSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var modelSettings = await _context.ModelSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        return new ModelModerationSettingsSnapshot
        {
            RequireModeration = systemSetup?.RequireModeration ?? true,
            AutoApproveModels = systemSetup?.AutoApproveModels ?? false,
            RequireUploadModeration = modelSettings?.RequireUploadModeration ?? true,
            RequireModeratorApproval = modelSettings?.RequireModeratorApproval ?? true,
            AutoApproveVerifiedUsers = modelSettings?.AutoApproveVerifiedUsers ?? false
        };
    }
}
