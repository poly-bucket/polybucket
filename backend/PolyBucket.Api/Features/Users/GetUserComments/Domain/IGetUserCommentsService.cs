using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUserComments.Domain;

public interface IGetUserCommentsService
{
    Task<GetUserCommentsResult> GetUserCommentsAsync(GetUserCommentsQuery query, CancellationToken cancellationToken = default);
}
