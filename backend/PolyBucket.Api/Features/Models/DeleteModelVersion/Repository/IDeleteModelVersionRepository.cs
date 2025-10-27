using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteModelVersion.Repository
{
    public interface IDeleteModelVersionRepository
    {
        Task<ModelVersion?> GetModelVersionAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken);
        Task DeleteModelVersionAsync(ModelVersion modelVersion, CancellationToken cancellationToken);
    }
} 