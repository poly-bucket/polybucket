using PolyBucket.Api.Common.Entities;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Federation.Domain
{
    public class FederatedInstance : Auditable
    {
        public new Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string SharedSecret { get; set; } = string.Empty;
        public FederationStatus Status { get; set; } = FederationStatus.Pending;
        public bool IsActive { get; set; } = true;
        public bool IsTrusted { get; set; } = false;
        public DateTime? LastSyncAt { get; set; }
        public DateTime? LastHeartbeatAt { get; set; }
        public string? LastError { get; set; }
        public int SyncIntervalMinutes { get; set; } = 60;
        public string? AdminContact { get; set; }
        public string? Version { get; set; }
        public int MaxModelsToSync { get; set; } = 1000;
        
        // Access control settings
        public string AllowedCategories { get; set; } = string.Empty; // JSON array of allowed category IDs
        public string AllowedRoles { get; set; } = string.Empty; // JSON array of role names that can sync
        public bool SyncPublicOnly { get; set; } = true;
        public bool SyncFeaturedOnly { get; set; } = false;
        
        // Statistics
        public int ModelsShared { get; set; } = 0;
        public int ModelsReceived { get; set; } = 0;
        public long TotalBytesTransferred { get; set; } = 0;
        
        // Navigation properties
        public ICollection<FederatedModel> SharedModels { get; set; } = new List<FederatedModel>();
        public ICollection<FederationHandshake> Handshakes { get; set; } = new List<FederationHandshake>();
        public ICollection<FederationAuditLog> AuditLogs { get; set; } = new List<FederationAuditLog>();
    }

    public enum FederationStatus
    {
        Pending = 0,
        Connecting = 1,
        Connected = 2,
        Failed = 3,
        Disabled = 4,
        Rejected = 5
    }
} 