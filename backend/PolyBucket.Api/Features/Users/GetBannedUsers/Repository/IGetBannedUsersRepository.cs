using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetBannedUsers.Http;

namespace PolyBucket.Api.Features.Users.GetBannedUsers.Repository;

public interface IGetBannedUsersRepository
{
    Task<BannedUsersListResponse> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
