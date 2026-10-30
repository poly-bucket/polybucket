using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Repository;
using PolyBucket.Api.Features.Reports.GetTopReportedComments.Http;

namespace PolyBucket.Api.Features.Reports.GetTopReportedComments.Repository;

public class GetTopReportedCommentsRepository(IGetReportAnalyticsRepository analyticsRepository) : IGetTopReportedCommentsRepository
{
    public async Task<TopReportedCommentsResponseDto> GetTopReportedCommentsAsync(int limit, CancellationToken cancellationToken = default)
    {
        var items = await analyticsRepository.GetTopReportedCommentsAsync(limit, cancellationToken);
        return new TopReportedCommentsResponseDto
        {
            Items = items.Select(i => new TopReportedCommentItemDto
            {
                Id = i.Id,
                Name = i.Name,
                ReportCount = i.ReportCount,
                LastReported = i.LastReported,
                IsResolved = i.IsResolved,
                Resolution = i.Resolution
            }).ToList()
        };
    }
}
