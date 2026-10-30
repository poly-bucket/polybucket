using PolyBucket.Api.Features.Reports.GetTopReportedUsers.Http;

namespace PolyBucket.Api.Features.Reports.GetTopReportedUsers.Repository;

public interface IGetTopReportedUsersRepository
{
    Task<TopReportedUsersResponseDto> GetTopReportedUsersAsync(int limit, CancellationToken cancellationToken = default);
}
