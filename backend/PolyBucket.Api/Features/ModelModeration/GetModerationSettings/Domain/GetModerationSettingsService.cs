using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Domain;

public class GetModerationSettingsService(IModelModerationSettingsProvider settingsProvider) : IGetModerationSettingsService
{
    private readonly IModelModerationSettingsProvider _settingsProvider = settingsProvider;

    public async Task<ModerationSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await _settingsProvider.GetSnapshotAsync(cancellationToken);
        return new ModerationSettingsDto
        {
            RequireModeration = snapshot.RequireModeration,
            AutoApproveModels = snapshot.AutoApproveModels,
            RequireUploadModeration = snapshot.RequireUploadModeration,
            RequireModeratorApproval = snapshot.RequireModeratorApproval,
            AutoApproveVerifiedUsers = snapshot.AutoApproveVerifiedUsers
        };
    }
}
