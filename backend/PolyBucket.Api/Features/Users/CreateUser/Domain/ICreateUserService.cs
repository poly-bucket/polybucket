using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.CreateUser.Domain;

public interface ICreateUserService
{
    Task<CreateUserCommandResponse> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken = default);
}
