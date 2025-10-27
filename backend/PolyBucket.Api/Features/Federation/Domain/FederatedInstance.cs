using PolyBucket.Api.Common.Entities;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Federation.Domain
{
    public class FederatedInstance : Auditable
    {
        public new Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string? PublicKey { get; set; }
        public string? SharedSecret { get; set; }
        public FederationStatus Status { get; set; } = FederationStatus.Pending;
        public DateTime? LastSyncAt { get; set; }
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;

        // Token Management Fields
        /// <summary>
        /// JWT token we send to the remote instance (encrypted at rest)
        /// </summary>
        public string? OurToken { get; set; }
        
        /// <summary>
        /// When our token expires
        /// </summary>
        public DateTime? OurTokenExpiry { get; set; }
        
        /// <summary>
        /// JWT token the remote instance sends to us (encrypted at rest)
        /// </summary>
        public string? TheirToken { get; set; }
        
        /// <summary>
        /// When their token expires
        /// </summary>
        public DateTime? TheirTokenExpiry { get; set; }
        
        /// <summary>
        /// Token renewal mode: Automatic, Timed, or Manual
        /// </summary>
        public TokenRenewalMode TokenRenewalMode { get; set; } = TokenRenewalMode.Automatic;

        // Sync Configuration Fields
        /// <summary>
        /// Sync mode: Manual or Automatic
        /// </summary>
        public SyncMode SyncMode { get; set; } = SyncMode.Manual;
        
        /// <summary>
        /// Sync interval: Daily, Weekly, or Monthly
        /// </summary>
        public SyncInterval? SyncInterval { get; set; }
        
        /// <summary>
        /// Whether to automatically import new models
        /// </summary>
        public bool AutoImportNewModels { get; set; } = false;
        
        /// <summary>
        /// When the next sync should occur
        /// </summary>
        public DateTime? NextSyncAt { get; set; }

        public virtual ICollection<FederatedModel> SharedModels { get; set; } = new List<FederatedModel>();
        public virtual ICollection<FederationHandshake> Handshakes { get; set; } = new List<FederationHandshake>();
        public virtual ICollection<FederationAuditLog> AuditLogs { get; set; } = new List<FederationAuditLog>();
    }
}
