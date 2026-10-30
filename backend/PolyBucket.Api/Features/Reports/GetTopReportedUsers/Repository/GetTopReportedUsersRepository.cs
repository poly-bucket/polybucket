using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Repository;
using PolyBucket.Api.Features.Reports.GetTopReportedUsers.Http;

namespace PolyBucket.Api.Features.Reports.GetTopReportedUsers.Repository;

public class GetTopReportedUsersRepository(IGetReportAnalyticsRepository analyticsRepository) : IGetTopReportedUsersRepository
{
    public async Task<TopReportedUsersResponseDto> GetTopReportedUsersAsync(int limit, CancellationToken cancellationToken = default)
    {
        var items = await analyticsRepository.GetTopReportedUsersAsync(limit, cancellationToken);
        return new TopReportedUsersResponseDto
        {
            Items = items.Select(i => new TopReportedUserItemDto
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
