using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.UpdateUserProfile.Domain;

public interface IUpdateUserProfileService
{
    Task UpdateAsync(UpdateUserProfileCommand command, CancellationToken cancellationToken = default);
}
