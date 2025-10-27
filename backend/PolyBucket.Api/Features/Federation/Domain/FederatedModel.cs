using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Shared.Domain;
using System;

namespace PolyBucket.Api.Features.Federation.Domain
{
    public class FederatedModel : Auditable
    {
        public new Guid Id { get; set; }
        
        // Local model reference (if this is a model we're sharing)
        public Guid? LocalModelId { get; set; }
        public Model? LocalModel { get; set; }
        
        // Remote model information (if this is a model from another instance)
        public Guid? RemoteModelId { get; set; }
        public string RemoteModelName { get; set; } = string.Empty;
        public string RemoteModelDescription { get; set; } = string.Empty;
        public string? RemoteAuthorName { get; set; }
        public DateTime RemoteCreatedAt { get; set; }
        public DateTime RemoteUpdatedAt { get; set; }
        public string? RemoteThumbnailUrl { get; set; }
        public int RemoteDownloads { get; set; }
        public int RemoteLikes { get; set; }
        public string RemoteCategories { get; set; } = string.Empty; // JSON array
        public string RemoteTags { get; set; } = string.Empty; // JSON array
        public string? RemoteLicense { get; set; }
        public bool RemoteIsNSFW { get; set; }
        public bool RemoteIsAIGenerated { get; set; }
        public long RemoteFileSize { get; set; }
        public string RemoteFileFormats { get; set; } = string.Empty; // JSON array of file extensions
        
        // Federation metadata
        public Guid FederatedInstanceId { get; set; }
        public FederatedInstance FederatedInstance { get; set; } = null!;
        public FederationDirection Direction { get; set; }
        public ModelSyncStatus SyncStatus { get; set; } = ModelSyncStatus.Pending;
        public DateTime? LastSyncAt { get; set; }
        public string? SyncError { get; set; }
        public string MetadataHash { get; set; } = string.Empty; // For detecting changes
        public bool IsVisible { get; set; } = true; // Local admin can hide federated models
        public int SyncPriority { get; set; } = 100; // Higher priority models sync first
        
        // File caching (for remote models)
        public bool HasLocalCache { get; set; } = false;
        public string? LocalCachePath { get; set; }
        public DateTime? CacheExpiresAt { get; set; }
        public long CachedFileSize { get; set; }
    }

    public enum FederationDirection
    {
        Outgoing = 1, // We're sharing this model to another instance
        Incoming = 2  // We're receiving this model from another instance
    }

    public enum ModelSyncStatus
    {
        Pending = 0,
        Syncing = 1,
        Synced = 2,
        Failed = 3,
        Skipped = 4,
        Deleted = 5
    }
} 