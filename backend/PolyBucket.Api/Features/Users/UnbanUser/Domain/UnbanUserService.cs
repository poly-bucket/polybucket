using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.UnbanUser.Repository;

namespace PolyBucket.Api.Features.Users.UnbanUser.Domain;

public class UnbanUserService(IUnbanUserRepository repository) : IUnbanUserService
{
    public async Task UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindByIdForUpdateAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!user.IsBanned)
        {
            throw new InvalidOperationException("User is not banned");
        }

        user.IsBanned = false;
        user.BannedAt = null;
        user.BannedById = null;
        user.BanReason = null;
        user.BanExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
    }
}
