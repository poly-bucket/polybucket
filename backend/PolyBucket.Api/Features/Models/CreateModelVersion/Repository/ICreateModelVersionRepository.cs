using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModelVersion.Repository
{
    public interface ICreateModelVersionRepository
    {
        Task<Model?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken);
        Task<ModelVersion> CreateModelVersionAsync(ModelVersion modelVersion, CancellationToken cancellationToken);
        Task<int> GetNextVersionNumberAsync(Guid modelId, CancellationToken cancellationToken);
    }
} 