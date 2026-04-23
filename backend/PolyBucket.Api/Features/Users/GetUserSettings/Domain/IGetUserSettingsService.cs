using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUserSettings.Domain;

public interface IGetUserSettingsService
{
    Task<GetUserSettingsResult> GetUserSettingsAsync(GetUserSettingsRequest request, CancellationToken cancellationToken = default);
}
