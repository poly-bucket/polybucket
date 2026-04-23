using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.UpdateModelVersion.Repository
{
    public interface IUpdateModelVersionRepository
    {
        Task<ModelVersion?> GetModelVersionAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken);
        Task<ModelVersion> UpdateModelVersionAsync(ModelVersion modelVersion, CancellationToken cancellationToken);
    }
} 