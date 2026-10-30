using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http;

namespace PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Repository;

public interface IGetModerationAuditLogsRepository
{
    Task<ModerationAuditResponseDto> GetAuditLogsAsync(GetModerationAuditLogsQuery query, CancellationToken cancellationToken = default);
}
