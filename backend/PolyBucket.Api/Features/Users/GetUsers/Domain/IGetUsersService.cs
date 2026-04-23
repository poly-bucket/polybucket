using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUsers.Domain;

public interface IGetUsersService
{
    Task<GetUsersResult> GetUsersAsync(GetUsersQuery query, CancellationToken cancellationToken = default);
}
