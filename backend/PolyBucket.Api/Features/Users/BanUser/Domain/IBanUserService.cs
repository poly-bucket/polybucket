using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.BanUser.Domain;

public interface IBanUserService
{
    Task BanUserAsync(Guid userId, Guid currentUserId, string reason, DateTime? expiresAt, CancellationToken cancellationToken = default);
}
