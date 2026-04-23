using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUserPrinters.Domain;

public interface IGetUserPrintersService
{
    Task<GetUserPrintersResult> GetUserPrintersAsync(GetUserPrintersQuery query, CancellationToken cancellationToken = default);
}
