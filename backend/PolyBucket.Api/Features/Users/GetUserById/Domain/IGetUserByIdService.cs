using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.GetUserById.Domain;

public interface IGetUserByIdService
{
    Task<GetUserByIdResult> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
