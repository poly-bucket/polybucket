using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetBannedUsers.Http;
using PolyBucket.Api.Features.Users.GetBannedUsers.Repository;

namespace PolyBucket.Api.Features.Users.GetBannedUsers.Domain;

public class GetBannedUsersService(IGetBannedUsersRepository repository) : IGetBannedUsersService
{
    public Task<BannedUsersListResponse> GetBannedUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return repository.GetPagedAsync(page, pageSize, cancellationToken);
    }
}
