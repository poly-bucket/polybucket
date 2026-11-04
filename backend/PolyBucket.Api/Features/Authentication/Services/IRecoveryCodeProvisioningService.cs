using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Authentication.Services;

public interface IRecoveryCodeProvisioningService
{
    Task<IReadOnlyList<string>> ProvisionForNewUserAsync(
        User user,
        Guid actorUserId,
        bool storePendingDisplayPayload,
        UserAuditAction auditAction,
        CancellationToken cancellationToken = default);
}
