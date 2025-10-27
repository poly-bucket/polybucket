using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Services
{
    public interface IFederationModelSyncService
    {
        Task<SyncResult> SyncModelToInstanceAsync(Guid modelId, Guid instanceId, Guid userId);
        Task<SyncResult> SyncInstanceModelsAsync(Guid instanceId, Guid userId);
    }
} 