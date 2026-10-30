using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.SkipPasswordChange.Repository;

public interface ISkipPasswordChangeRepository
{
    Task<User?> GetUserForSkipAsync(Guid userId, CancellationToken cancellationToken);
    Task MarkAdminConfiguredAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
