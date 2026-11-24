using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Services;

public interface IPasswordPolicyService
{
    Task<(bool IsValid, string Message)> ValidateAsync(string password, CancellationToken cancellationToken = default);
}