using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http;
using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Repository;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Domain;

public interface IGetModerationAuditLogsService
{
    Task<ModerationAuditResponseDto?> GetAuditLogsAsync(Guid currentUserId, GetModerationAuditLogsQuery query, CancellationToken cancellationToken = default);
}

public class GetModerationAuditLogsService(
    IGetModerationAuditLogsRepository repository,
    PolyBucketDbContext context) : IGetModerationAuditLogsService
{
    public async Task<ModerationAuditResponseDto?> GetAuditLogsAsync(
        Guid currentUserId,
        GetModerationAuditLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (user == null || user.Role == null || user.Role.Name != "Admin")
        {
            return null;
        }

        return await repository.GetAuditLogsAsync(query, cancellationToken);
    }
}
