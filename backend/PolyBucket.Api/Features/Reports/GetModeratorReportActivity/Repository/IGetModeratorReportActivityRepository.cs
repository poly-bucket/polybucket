using PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Http;
namespace PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Repository;
public interface IGetModeratorReportActivityRepository { Task<ModeratorReportActivityResponseDto> GetModeratorActivityAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default); }
