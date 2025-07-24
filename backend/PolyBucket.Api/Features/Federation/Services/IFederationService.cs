using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Services
{
    public interface IFederationService
    {
        Task<FederationSettings?> GetFederationSettingsAsync();
        Task<FederationSettings> UpdateFederationSettingsAsync(FederationSettings settings, Guid userId);
        Task<string> GenerateInviteUrlAsync(Guid userId, DateTime? expiresAt = null);
        Task<FederatedInstance> ConnectToInstanceAsync(string inviteUrl, Guid userId);
        Task<HandshakeResult> InitiateHandshakeAsync(Guid instanceId, Guid userId);
        Task<IEnumerable<Model>> GetSharableModelsAsync(Guid instanceId, Guid userId);
        Task DisconnectFromInstanceAsync(Guid instanceId, Guid userId);
    }
} 