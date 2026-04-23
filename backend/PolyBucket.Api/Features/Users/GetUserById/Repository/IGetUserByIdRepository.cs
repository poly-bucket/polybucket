using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Users.GetUserById.Repository;

public interface IGetUserByIdRepository
{
    Task<User?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
}
