using PolyBucket.Api.Features.Reports.GetTopReportedComments.Http;
using PolyBucket.Api.Features.Reports.GetTopReportedComments.Repository;

namespace PolyBucket.Api.Features.Reports.GetTopReportedComments.Domain;

public interface IGetTopReportedCommentsService
{
    Task<TopReportedCommentsResponseDto> GetTopReportedCommentsAsync(int limit, CancellationToken cancellationToken = default);
}

public class GetTopReportedCommentsService(IGetTopReportedCommentsRepository repository) : IGetTopReportedCommentsService
{
    public Task<TopReportedCommentsResponseDto> GetTopReportedCommentsAsync(int limit, CancellationToken cancellationToken = default) =>
        repository.GetTopReportedCommentsAsync(limit, cancellationToken);
}
