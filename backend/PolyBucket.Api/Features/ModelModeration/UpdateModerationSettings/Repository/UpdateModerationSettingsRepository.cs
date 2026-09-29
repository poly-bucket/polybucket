using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Repository;

public class UpdateModerationSettingsRepository(PolyBucketDbContext context) : IUpdateModerationSettingsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task UpdateAsync(ModerationSettingsDto settings, CancellationToken cancellationToken = default)
    {
        var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
        if (systemSetup != null)
        {
            systemSetup.RequireModeration = settings.RequireModeration;
            systemSetup.AutoApproveModels = settings.AutoApproveModels;
            systemSetup.UpdatedAt = System.DateTime.UtcNow;
        }

        var modelSettings = await _context.ModelSettings.FirstOrDefaultAsync(cancellationToken);
        if (modelSettings != null)
        {
            modelSettings.RequireUploadModeration = settings.RequireUploadModeration;
            modelSettings.RequireModeratorApproval = settings.RequireModeratorApproval;
            modelSettings.AutoApproveVerifiedUsers = settings.AutoApproveVerifiedUsers;
            modelSettings.UpdatedAt = System.DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
