using PolyBucket.Api.Features.Reports.GetTopReportedUsers.Http;
using PolyBucket.Api.Features.Reports.GetTopReportedUsers.Repository;

namespace PolyBucket.Api.Features.Reports.GetTopReportedUsers.Domain;

public interface IGetTopReportedUsersService
{
    Task<TopReportedUsersResponseDto> GetTopReportedUsersAsync(int limit, CancellationToken cancellationToken = default);
}

public class GetTopReportedUsersService(IGetTopReportedUsersRepository repository) : IGetTopReportedUsersService
{
    public Task<TopReportedUsersResponseDto> GetTopReportedUsersAsync(int limit, CancellationToken cancellationToken = default) =>
        repository.GetTopReportedUsersAsync(limit, cancellationToken);
}
