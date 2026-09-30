using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelVersions.Domain;

public interface IGetModelVersionsService
{
    Task<GetModelVersionsResponse?> GetModelVersionsAsync(Guid modelId, Guid? viewerId, CancellationToken cancellationToken);
}
