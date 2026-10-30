using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Services;

public interface IUserRegistrationGate
{
    Task<bool> IsRegistrationAllowedAsync(CancellationToken cancellationToken = default);

    Task EnsureRegistrationAllowedAsync(CancellationToken cancellationToken = default);
}
