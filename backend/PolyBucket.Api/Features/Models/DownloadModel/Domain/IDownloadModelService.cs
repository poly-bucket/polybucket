using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.DownloadModel.Domain;

public interface IDownloadModelService
{
    Task<DownloadModelOutcome> DownloadAsync(
        Guid id,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}
