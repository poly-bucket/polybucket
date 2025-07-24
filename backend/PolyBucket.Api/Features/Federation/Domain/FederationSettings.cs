using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Federation.Domain
{
    public class FederationSettings : BaseEntity
    {
        public bool IsFederationEnabled { get; set; } = false;
        public string InstanceName { get; set; } = string.Empty;
        public string InstanceDescription { get; set; } = string.Empty;
        public string AdminContact { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        
        // Security settings
        public string PrivateKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public int KeyRotationIntervalDays { get; set; } = 90;
        public DateTime? LastKeyRotation { get; set; }
        public bool RequireHttps { get; set; } = true;
        public bool AllowSelfSignedCertificates { get; set; } = false;
        public int HandshakeTimeoutMinutes { get; set; } = 5;
        
        // Sync settings
        public int DefaultSyncIntervalMinutes { get; set; } = 60;
        public int MaxConcurrentSyncs { get; set; } = 5;
        public int MaxModelsPerInstance { get; set; } = 10000;
        public long MaxModelFileSizeBytes { get; set; } = 100L * 1024 * 1024; // 100MB
        public int ModelCacheRetentionDays { get; set; } = 30;
        public bool AutoAcceptHandshakes { get; set; } = false;
        
        // Access control
        public bool SharePublicModelsOnly { get; set; } = true;
        public bool ShareFeaturedModelsOnly { get; set; } = false;
        public string AllowedFileTypes { get; set; } = string.Empty; // JSON array of allowed extensions
        public string BlockedCategories { get; set; } = string.Empty; // JSON array of blocked category IDs
        public bool AllowNSFWContent { get; set; } = false;
        public bool RequireApprovalForNewInstances { get; set; } = true;
        
        // Rate limiting
        public int MaxRequestsPerMinute { get; set; } = 100;
        public int MaxDownloadsPerHour { get; set; } = 1000;
        public long MaxBandwidthPerDay { get; set; } = 10L * 1024 * 1024 * 1024; // 10GB
        
        // Monitoring and health
        public int HeartbeatIntervalMinutes { get; set; } = 15;
        public int HealthCheckTimeoutSeconds { get; set; } = 30;
        public bool EnableMetrics { get; set; } = true;
        public bool EnableDetailedLogging { get; set; } = false;
        
        // Discovery and announcement
        public string FederationInviteUrl { get; set; } = string.Empty;
        public DateTime? InviteUrlGeneratedAt { get; set; }
        public DateTime? InviteUrlExpiresAt { get; set; }
        public bool EnableDiscovery { get; set; } = false;
        public string? DiscoveryTags { get; set; } // JSON array of tags for discovery
        
        // Backup and recovery
        public bool EnableAutomaticBackup { get; set; } = true;
        public int BackupRetentionDays { get; set; } = 30;
        public DateTime? LastBackupAt { get; set; }
        
        // Version and compatibility
        public string FederationProtocolVersion { get; set; } = "1.0";
        public string? SupportedVersions { get; set; } // JSON array of supported protocol versions
        
        // Statistics
        public int TotalConnectedInstances { get; set; } = 0;
        public int TotalModelsShared { get; set; } = 0;
        public int TotalModelsReceived { get; set; } = 0;
        public long TotalBytesTransferred { get; set; } = 0;
        public DateTime? LastStatsUpdate { get; set; }
    }
} 