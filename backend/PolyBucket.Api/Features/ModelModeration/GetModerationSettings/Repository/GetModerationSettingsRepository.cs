using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Http;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Repository;

public class GetModerationSettingsRepository(PolyBucketDbContext context) : IGetModerationSettingsRepository
{
    public async Task<GetModerationSettingsResponseDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var systemSetup = await context.SystemSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var modelSettings = await context.ModelSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        return new GetModerationSettingsResponseDto
        {
            RequireModeration = systemSetup?.RequireModeration ?? true,
            AutoApproveModels = systemSetup?.AutoApproveModels ?? false,
            RequireUploadModeration = modelSettings?.RequireUploadModeration ?? true,
            RequireModeratorApproval = modelSettings?.RequireModeratorApproval ?? true,
            AutoApproveVerifiedUsers = modelSettings?.AutoApproveVerifiedUsers ?? false
        };
    }
}
