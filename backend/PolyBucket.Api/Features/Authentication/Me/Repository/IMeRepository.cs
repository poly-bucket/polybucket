using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.Me.Repository;

public interface IMeRepository
{
    Task<User?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
