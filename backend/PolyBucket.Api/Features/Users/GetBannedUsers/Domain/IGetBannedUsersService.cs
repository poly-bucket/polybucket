using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetBannedUsers.Http;

namespace PolyBucket.Api.Features.Users.GetBannedUsers.Domain;

public interface IGetBannedUsersService
{
    Task<BannedUsersListResponse> GetBannedUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
