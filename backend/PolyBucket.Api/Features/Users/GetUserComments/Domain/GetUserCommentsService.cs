using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserComments.Repository;

namespace PolyBucket.Api.Features.Users.GetUserComments.Domain;

public class GetUserCommentsService(IGetUserCommentsRepository repository) : IGetUserCommentsService
{
    public Task<GetUserCommentsResult> GetUserCommentsAsync(GetUserCommentsQuery query, CancellationToken cancellationToken = default)
    {
        return repository.GetUserCommentsAsync(query, cancellationToken);
    }
}
