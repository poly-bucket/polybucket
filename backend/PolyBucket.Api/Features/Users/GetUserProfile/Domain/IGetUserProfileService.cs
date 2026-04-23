using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUserProfile.Domain;

public interface IGetUserProfileService
{
    Task<object> GetUserProfileAsync(GetUserProfileQuery query, CancellationToken cancellationToken = default);
}
