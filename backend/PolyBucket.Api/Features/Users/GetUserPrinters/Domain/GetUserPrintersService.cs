using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserPrinters.Repository;

namespace PolyBucket.Api.Features.Users.GetUserPrinters.Domain;

public class GetUserPrintersService(IGetUserPrintersRepository repository) : IGetUserPrintersService
{
    public Task<GetUserPrintersResult> GetUserPrintersAsync(GetUserPrintersQuery query, CancellationToken cancellationToken = default)
    {
        return repository.GetUserPrintersAsync(query, cancellationToken);
    }
}
