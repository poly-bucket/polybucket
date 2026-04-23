using PolyBucket.Api.Common.Models;
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