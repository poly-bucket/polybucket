using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http;
using PolyBucket.Database.Entities.ModelModeration;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Repository;

public class GetModerationAuditLogsRepository(PolyBucketDbContext context) : IGetModerationAuditLogsRepository
{
    public async Task<ModerationAuditResponseDto> GetAuditLogsAsync(GetModerationAuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = context.ModerationAuditLogs
            .Include(log => log.PerformedByUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query.Action) &&
            Enum.TryParse<ModerationAction>(query.Action, true, out var actionEnum))
        {
            dbQuery = dbQuery.Where(log => log.Action == actionEnum);
        }

        if (!string.IsNullOrEmpty(query.UserId) && Guid.TryParse(query.UserId, out var userGuid))
        {
            dbQuery = dbQuery.Where(log => log.PerformedByUserId == userGuid);
        }

        if (!string.IsNullOrEmpty(query.ModelId) && Guid.TryParse(query.ModelId, out var modelGuid))
        {
            dbQuery = dbQuery.Where(log => log.ModelId == modelGuid);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        var logs = await dbQuery
            .OrderByDescending(log => log.PerformedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(log => new ModerationAuditLogItemDto
            {
                Id = log.Id.ToString(),
                ModelId = log.ModelId.ToString(),
                PerformedByUserId = log.PerformedByUserId.HasValue ? log.PerformedByUserId.Value.ToString() : string.Empty,
                PerformedByUser = new ModerationAuditUserInfoDto
                {
                    Username = log.PerformedByUser != null ? log.PerformedByUser.Username : string.Empty,
                    Email = log.PerformedByUser != null ? log.PerformedByUser.Email : string.Empty
                },
                Action = log.Action.ToString(),
                PreviousValues = log.PreviousValues,
                NewValues = log.NewValues,
                ModerationNotes = log.ModerationNotes,
                PerformedAt = log.PerformedAt,
                IPAddress = log.IPAddress,
                UserAgent = log.UserAgent
            })
            .ToListAsync(cancellationToken);

        return new ModerationAuditResponseDto
        {
            Logs = logs,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };
    }
}
