using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Domain
{
    public interface IFederationPlugin : IPlugin
    {
        /// <summary>
        /// Validates whether a model should be shared with a specific federated instance
        /// </summary>
        Task<bool> ShouldShareModelAsync(Model model, FederatedInstance instance, Guid userId);
        
        /// <summary>
        /// Validates whether a model from a federated instance should be accepted
        /// </summary>
        Task<bool> ShouldAcceptModelAsync(FederatedModel federatedModel, FederatedInstance instance);
        
        /// <summary>
        /// Transforms model metadata before sharing with another instance
        /// </summary>
        Task<Dictionary<string, object>> TransformModelMetadataAsync(Model model, FederatedInstance targetInstance);
        
        /// <summary>
        /// Validates and processes incoming model metadata from another instance
        /// </summary>
        Task<Dictionary<string, object>> ProcessIncomingModelMetadataAsync(Dictionary<string, object> metadata, FederatedInstance sourceInstance);
        
        /// <summary>
        /// Validates a handshake request from another instance
        /// </summary>
        Task<HandshakeValidationResult> ValidateHandshakeAsync(FederationHandshake handshake, FederatedInstance instance);
        
        /// <summary>
        /// Generates additional security challenges for handshake
        /// </summary>
        Task<string?> GenerateSecurityChallengeAsync(FederatedInstance instance);
        
        /// <summary>
        /// Validates security challenge response
        /// </summary>
        Task<bool> ValidateSecurityResponseAsync(string challenge, string response, FederatedInstance instance);
        
        /// <summary>
        /// Filters which models are visible in federation discovery
        /// </summary>
        Task<IEnumerable<Model>> FilterDiscoverableModelsAsync(IEnumerable<Model> models, FederatedInstance requestingInstance);
        
        /// <summary>
        /// Enriches model data with plugin-specific information
        /// </summary>
        Task<Dictionary<string, object>> EnrichModelDataAsync(Model model, FederatedInstance targetInstance);
        
        /// <summary>
        /// Handles custom federation events
        /// </summary>
        Task OnFederationEventAsync(FederationEvent federationEvent);
        
        /// <summary>
        /// Validates federation settings changes
        /// </summary>
        Task<SettingsValidationResult> ValidateFederationSettingsAsync(FederationSettings oldSettings, FederationSettings newSettings, Guid userId);
        
        /// <summary>
        /// Provides custom sync strategies for specific model types
        /// </summary>
        Task<SyncStrategy?> GetSyncStrategyAsync(Model model, FederatedInstance targetInstance);
        
        /// <summary>
        /// Handles pre-sync model processing
        /// </summary>
        Task OnPreSyncAsync(FederatedModel federatedModel);
        
        /// <summary>
        /// Handles post-sync model processing
        /// </summary>
        Task OnPostSyncAsync(FederatedModel federatedModel, bool successful);
        
        /// <summary>
        /// Provides custom audit log enrichment
        /// </summary>
        Task<Dictionary<string, object>?> EnrichAuditLogAsync(FederationAuditLog auditLog);
    }

    public class HandshakeValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class SettingsValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class SyncStrategy
    {
        public string Name { get; set; } = string.Empty;
        public int Priority { get; set; } = 100;
        public TimeSpan SyncInterval { get; set; } = TimeSpan.FromHours(1);
        public bool RequiresFullSync { get; set; } = false;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class FederationEvent
    {
        public string EventType { get; set; } = string.Empty;
        public Guid? FederatedInstanceId { get; set; }
        public Guid? ModelId { get; set; }
        public Guid? UserId { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
} 