using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.BanUser.Repository;

namespace PolyBucket.Api.Features.Users.BanUser.Domain;

public class BanUserService(IBanUserRepository repository) : IBanUserService
{
    public async Task BanUserAsync(Guid userId, Guid currentUserId, string reason, DateTime? expiresAt, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindByIdForUpdateAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.IsBanned)
        {
            throw new InvalidOperationException("User is already banned");
        }

        if (user.Id == currentUserId)
        {
            throw new InvalidOperationException("Cannot ban yourself");
        }

        user.IsBanned = true;
        user.BannedAt = DateTime.UtcNow;
        user.BannedById = currentUserId;
        user.BanReason = reason;
        user.BanExpiresAt = expiresAt;
        user.UpdatedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
    }
}
