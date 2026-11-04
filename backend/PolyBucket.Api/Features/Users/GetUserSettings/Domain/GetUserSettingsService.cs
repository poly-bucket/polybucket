using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserSettings.Repository;

namespace PolyBucket.Api.Features.Users.GetUserSettings.Domain;

public class GetUserSettingsService(IGetUserSettingsRepository repository) : IGetUserSettingsService
{
    public async Task<GetUserSettingsResult> GetUserSettingsAsync(GetUserSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetSettingsByUserIdAsync(request.UserId, cancellationToken);
        if (settings == null)
        {
            settings = await repository.CreateDefaultSettingsForUserAsync(request.UserId, cancellationToken);
        }

        return new GetUserSettingsResult { Settings = settings };
    }
}
