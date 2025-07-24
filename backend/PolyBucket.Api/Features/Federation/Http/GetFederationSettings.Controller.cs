using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Services;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    [ApiController]
    [Route("api/federation/settings")]
    [Authorize]
    public class GetFederationSettingsController(IFederationService federationService) : ControllerBase
    {
        private readonly IFederationService _federationService = federationService;

        /// <summary>
        /// Get federation settings
        /// </summary>
        [HttpGet]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(FederationSettingsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FederationSettingsDto>> GetFederationSettings()
        {
            var settings = await _federationService.GetFederationSettingsAsync();
            
            if (settings == null)
            {
                return NotFound("Federation settings not configured");
            }

            var dto = new FederationSettingsDto
            {
                Id = settings.Id,
                IsFederationEnabled = settings.IsFederationEnabled,
                InstanceName = settings.InstanceName,
                InstanceDescription = settings.InstanceDescription,
                AdminContact = settings.AdminContact,
                BaseUrl = settings.BaseUrl,
                KeyRotationIntervalDays = settings.KeyRotationIntervalDays,
                LastKeyRotation = settings.LastKeyRotation,
                RequireHttps = settings.RequireHttps,
                AllowSelfSignedCertificates = settings.AllowSelfSignedCertificates,
                HandshakeTimeoutMinutes = settings.HandshakeTimeoutMinutes,
                DefaultSyncIntervalMinutes = settings.DefaultSyncIntervalMinutes,
                MaxConcurrentSyncs = settings.MaxConcurrentSyncs,
                MaxModelsPerInstance = settings.MaxModelsPerInstance,
                MaxModelFileSizeBytes = settings.MaxModelFileSizeBytes,
                ModelCacheRetentionDays = settings.ModelCacheRetentionDays,
                AutoAcceptHandshakes = settings.AutoAcceptHandshakes,
                SharePublicModelsOnly = settings.SharePublicModelsOnly,
                ShareFeaturedModelsOnly = settings.ShareFeaturedModelsOnly,
                AllowedFileTypes = settings.AllowedFileTypes,
                BlockedCategories = settings.BlockedCategories,
                AllowNSFWContent = settings.AllowNSFWContent,
                RequireApprovalForNewInstances = settings.RequireApprovalForNewInstances,
                MaxRequestsPerMinute = settings.MaxRequestsPerMinute,
                MaxDownloadsPerHour = settings.MaxDownloadsPerHour,
                MaxBandwidthPerDay = settings.MaxBandwidthPerDay,
                HeartbeatIntervalMinutes = settings.HeartbeatIntervalMinutes,
                HealthCheckTimeoutSeconds = settings.HealthCheckTimeoutSeconds,
                EnableMetrics = settings.EnableMetrics,
                EnableDetailedLogging = settings.EnableDetailedLogging,
                FederationInviteUrl = settings.FederationInviteUrl,
                InviteUrlGeneratedAt = settings.InviteUrlGeneratedAt,
                InviteUrlExpiresAt = settings.InviteUrlExpiresAt,
                EnableDiscovery = settings.EnableDiscovery,
                DiscoveryTags = settings.DiscoveryTags,
                EnableAutomaticBackup = settings.EnableAutomaticBackup,
                BackupRetentionDays = settings.BackupRetentionDays,
                LastBackupAt = settings.LastBackupAt,
                FederationProtocolVersion = settings.FederationProtocolVersion,
                SupportedVersions = settings.SupportedVersions,
                TotalConnectedInstances = settings.TotalConnectedInstances,
                TotalModelsShared = settings.TotalModelsShared,
                TotalModelsReceived = settings.TotalModelsReceived,
                TotalBytesTransferred = settings.TotalBytesTransferred,
                LastStatsUpdate = settings.LastStatsUpdate
            };

            return Ok(dto);
        }
    }

    public class FederationSettingsDto
    {
        public System.Guid Id { get; set; }
        public bool IsFederationEnabled { get; set; }
        public string InstanceName { get; set; } = string.Empty;
        public string InstanceDescription { get; set; } = string.Empty;
        public string AdminContact { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int KeyRotationIntervalDays { get; set; }
        public System.DateTime? LastKeyRotation { get; set; }
        public bool RequireHttps { get; set; }
        public bool AllowSelfSignedCertificates { get; set; }
        public int HandshakeTimeoutMinutes { get; set; }
        public int DefaultSyncIntervalMinutes { get; set; }
        public int MaxConcurrentSyncs { get; set; }
        public int MaxModelsPerInstance { get; set; }
        public long MaxModelFileSizeBytes { get; set; }
        public int ModelCacheRetentionDays { get; set; }
        public bool AutoAcceptHandshakes { get; set; }
        public bool SharePublicModelsOnly { get; set; }
        public bool ShareFeaturedModelsOnly { get; set; }
        public string AllowedFileTypes { get; set; } = string.Empty;
        public string BlockedCategories { get; set; } = string.Empty;
        public bool AllowNSFWContent { get; set; }
        public bool RequireApprovalForNewInstances { get; set; }
        public int MaxRequestsPerMinute { get; set; }
        public int MaxDownloadsPerHour { get; set; }
        public long MaxBandwidthPerDay { get; set; }
        public int HeartbeatIntervalMinutes { get; set; }
        public int HealthCheckTimeoutSeconds { get; set; }
        public bool EnableMetrics { get; set; }
        public bool EnableDetailedLogging { get; set; }
        public string FederationInviteUrl { get; set; } = string.Empty;
        public System.DateTime? InviteUrlGeneratedAt { get; set; }
        public System.DateTime? InviteUrlExpiresAt { get; set; }
        public bool EnableDiscovery { get; set; }
        public string? DiscoveryTags { get; set; }
        public bool EnableAutomaticBackup { get; set; }
        public int BackupRetentionDays { get; set; }
        public System.DateTime? LastBackupAt { get; set; }
        public string FederationProtocolVersion { get; set; } = string.Empty;
        public string? SupportedVersions { get; set; }
        public int TotalConnectedInstances { get; set; }
        public int TotalModelsShared { get; set; }
        public int TotalModelsReceived { get; set; }
        public long TotalBytesTransferred { get; set; }
        public System.DateTime? LastStatsUpdate { get; set; }
    }
} 