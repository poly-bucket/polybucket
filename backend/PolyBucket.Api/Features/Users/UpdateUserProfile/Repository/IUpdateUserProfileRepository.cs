using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Users.UpdateUserProfile.Repository;

public interface IUpdateUserProfileRepository
{
    Task<User?> GetUserByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
