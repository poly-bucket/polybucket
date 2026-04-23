using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserById.Repository;

namespace PolyBucket.Api.Features.Users.GetUserById.Domain;

public class GetUserByIdService(IGetUserByIdRepository repository) : IGetUserByIdService
{
    public async Task<GetUserByIdResult> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsNoTrackingAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        return new GetUserByIdResult
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username
        };
    }
}
