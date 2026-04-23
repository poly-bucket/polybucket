using PolyBucket.Api.Features.Users.GetUsers.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUsers.Repository;

public interface IGetUsersRepository
{
    Task<GetUsersResult> GetPagedAsync(GetUsersQuery query, CancellationToken cancellationToken = default);
}
