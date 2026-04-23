using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserPrinters.Domain;

namespace PolyBucket.Api.Features.Users.GetUserPrinters.Repository;

public interface IGetUserPrintersRepository
{
    Task<GetUserPrintersResult> GetUserPrintersAsync(GetUserPrintersQuery query, CancellationToken cancellationToken = default);
}
