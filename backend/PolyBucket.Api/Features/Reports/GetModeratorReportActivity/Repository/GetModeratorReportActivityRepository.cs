using PolyBucket.Api.Features.Reports.GetReportAnalytics.Repository;
using PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Http;
namespace PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Repository;
public class GetModeratorReportActivityRepository(IGetReportAnalyticsRepository analyticsRepository) : IGetModeratorReportActivityRepository {
    public async Task<ModeratorReportActivityResponseDto> GetModeratorActivityAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default) {
        var activity = await analyticsRepository.GetModeratorActivityAsync(fromDate, toDate, cancellationToken);
        return new ModeratorReportActivityResponseDto { Activity = activity.Select(a => new ModeratorReportActivityItemDto { ModeratorId = a.ModeratorId, ModeratorName = a.ModeratorName, ReportsResolved = a.ReportsResolved, ReportsDismissed = a.ReportsDismissed, ReportsArchived = a.ReportsArchived, LastActivity = a.LastActivity, AverageResolutionTime = a.AverageResolutionTime }).ToList() };
    }
}
