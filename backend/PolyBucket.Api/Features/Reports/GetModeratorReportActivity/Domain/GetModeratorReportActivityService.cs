using PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Http;
using PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Repository;
namespace PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Domain;
public interface IGetModeratorReportActivityService { Task<ModeratorReportActivityResponseDto> GetModeratorActivityAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default); }
public class GetModeratorReportActivityService(IGetModeratorReportActivityRepository repository) : IGetModeratorReportActivityService {
    public Task<ModeratorReportActivityResponseDto> GetModeratorActivityAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default) => repository.GetModeratorActivityAsync(fromDate, toDate, cancellationToken);
}
