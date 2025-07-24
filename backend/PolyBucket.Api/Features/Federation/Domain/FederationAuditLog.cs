using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Federation.Domain
{
    public class FederationAuditLog : Auditable
    {
        public new Guid Id { get; set; }
        
        public Guid? FederatedInstanceId { get; set; }
        public FederatedInstance? FederatedInstance { get; set; }
        
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        
        public FederationAction Action { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Details { get; set; } // JSON object with additional details
        
        // Request/Response information
        public string? RequestId { get; set; }
        public string? HttpMethod { get; set; }
        public string? EndpointPath { get; set; }
        public int? HttpStatusCode { get; set; }
        public string? RequestIpAddress { get; set; }
        public string? UserAgent { get; set; }
        
        // Data affected
        public string? AffectedResourceType { get; set; }
        public string? AffectedResourceId { get; set; }
        public string? PreviousValues { get; set; } // JSON object
        public string? NewValues { get; set; } // JSON object
        
        // Performance and metrics
        public long? ResponseTimeMs { get; set; }
        public long? DataTransferredBytes { get; set; }
        
        // Security information
        public bool IsSuccessful { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public string? SecurityFlags { get; set; } // JSON array of security-related flags
        public string? RiskLevel { get; set; } // Low, Medium, High, Critical
        
        // Metadata
        public string? Metadata { get; set; } // JSON object for extensible properties
        public bool IsInternal { get; set; } = false; // Internal system actions vs external API calls
        public string? CorrelationId { get; set; }
        public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
    }

    public enum FederationAction
    {
        // Instance management
        InstanceAdded = 100,
        InstanceUpdated = 101,
        InstanceRemoved = 102,
        InstanceConnected = 103,
        InstanceDisconnected = 104,
        InstanceBlocked = 105,
        InstanceUnblocked = 106,
        
        // Handshake operations
        HandshakeInitiated = 200,
        HandshakeReceived = 201,
        HandshakeCompleted = 202,
        HandshakeFailed = 203,
        HandshakeRejected = 204,
        
        // Model synchronization
        ModelShared = 300,
        ModelReceived = 301,
        ModelUpdated = 302,
        ModelRemoved = 303,
        ModelSyncFailed = 304,
        ModelCached = 305,
        ModelCacheExpired = 306,
        
        // Settings and configuration
        SettingsUpdated = 400,
        KeysRotated = 401,
        PermissionsChanged = 402,
        FilteringRulesUpdated = 403,
        
        // Security events
        UnauthorizedAccess = 500,
        InvalidSignature = 501,
        ExpiredToken = 502,
        RateLimitExceeded = 503,
        SuspiciousActivity = 504,
        SecurityScanCompleted = 505,
        
        // System events
        FederationEnabled = 600,
        FederationDisabled = 601,
        BackupCreated = 602,
        BackupRestored = 603,
        MaintenanceStarted = 604,
        MaintenanceCompleted = 605,
        
        // API operations
        ApiKeyGenerated = 700,
        ApiKeyRevoked = 701,
        ApiCallMade = 702,
        ApiCallReceived = 703,
        BulkOperationStarted = 704,
        BulkOperationCompleted = 705
    }
} 