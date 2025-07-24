using PolyBucket.Api.Features.Federation.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Repository
{
    public interface IFederationRepository
    {
        // Federation Settings
        Task<FederationSettings?> GetFederationSettingsAsync();
        Task<FederationSettings> UpdateFederationSettingsAsync(FederationSettings settings);
        
        // Federated Instances
        Task<FederatedInstance?> GetFederatedInstanceAsync(Guid id);
        Task<IEnumerable<FederatedInstance>> GetFederatedInstancesAsync();
        Task<FederatedInstance> AddFederatedInstanceAsync(FederatedInstance instance);
        Task<FederatedInstance> UpdateFederatedInstanceAsync(FederatedInstance instance);
        Task DeleteFederatedInstanceAsync(Guid id);
        
        // Federated Models
        Task<FederatedModel?> GetFederatedModelAsync(Guid id);
        Task<IEnumerable<FederatedModel>> GetFederatedModelsAsync(Guid instanceId);
        Task<FederatedModel> AddFederatedModelAsync(FederatedModel model);
        Task<FederatedModel> UpdateFederatedModelAsync(FederatedModel model);
        Task DeleteFederatedModelAsync(Guid id);
        
        // Handshakes
        Task<FederationHandshake?> GetHandshakeAsync(Guid id);
        Task<IEnumerable<FederationHandshake>> GetHandshakesAsync(Guid instanceId);
        Task<FederationHandshake> AddHandshakeAsync(FederationHandshake handshake);
        Task<FederationHandshake> UpdateHandshakeAsync(FederationHandshake handshake);
        
        // Audit Logs
        Task<FederationAuditLog> AddAuditLogAsync(FederationAuditLog auditLog);
        Task<IEnumerable<FederationAuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50);
        Task<IEnumerable<FederationAuditLog>> GetAuditLogsForInstanceAsync(Guid instanceId, int page = 1, int pageSize = 50);
    }
} 