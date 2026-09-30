using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.GetModelVersions.Domain;

namespace PolyBucket.Api.Features.Models.GetModelVersions.Repository;

public interface IGetModelVersionsRepository
{
    Task<ModelVersionsAccessInfo?> GetAccessInfoAsync(Guid modelId, CancellationToken cancellationToken);

    Task<List<ModelVersion>> GetVersionsAsync(Guid modelId, CancellationToken cancellationToken);
}
