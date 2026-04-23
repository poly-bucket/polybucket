using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserComments.Domain;

namespace PolyBucket.Api.Features.Users.GetUserComments.Repository;

public interface IGetUserCommentsRepository
{
    Task<GetUserCommentsResult> GetUserCommentsAsync(GetUserCommentsQuery query, CancellationToken cancellationToken = default);
}
