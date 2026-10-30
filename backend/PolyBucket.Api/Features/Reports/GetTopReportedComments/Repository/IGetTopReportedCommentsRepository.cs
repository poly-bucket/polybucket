using PolyBucket.Api.Features.Reports.GetTopReportedComments.Http;

namespace PolyBucket.Api.Features.Reports.GetTopReportedComments.Repository;

public interface IGetTopReportedCommentsRepository
{
    Task<TopReportedCommentsResponseDto> GetTopReportedCommentsAsync(int limit, CancellationToken cancellationToken = default);
}
