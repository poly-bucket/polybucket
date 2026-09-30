using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Users.MarkEmailVerified.Repository;

namespace PolyBucket.Api.Features.Users.MarkEmailVerified.Domain;

public class MarkEmailVerifiedService(
    IMarkEmailVerifiedRepository repository,
    TimeProvider timeProvider,
    ILogger<MarkEmailVerifiedService> logger) : IMarkEmailVerifiedService
{
    public async Task<DateTime> MarkEmailVerifiedAsync(Guid userId, Guid performedByUserId, ClientRequestInfo client, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindByIdForUpdateAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found");

        if (user.EmailVerifiedAt.HasValue)
        {
            return user.EmailVerifiedAt.Value;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        user.EmailVerifiedAt = now;
        user.UpdatedAt = now;
        repository.AddAuditLog(new UserAuditLog
        {
            UserId = user.Id,
            PerformedByUserId = performedByUserId,
            Action = UserAuditAction.EmailMarkedVerified,
            Details = $"Email {user.Email} marked as verified by an administrator",
            IpAddress = client.IpAddress,
            CreatedAt = now
        });
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} email marked verified by {AdminId}", user.Id, performedByUserId);
        return now;
    }
}
