using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Services;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    [ApiController]
    [Route("api/federation/settings")]
    [Authorize]
    public class UpdateFederationSettingsController(IFederationService federationService) : ControllerBase
    {
        private readonly IFederationService _federationService = federationService;

        /// <summary>
        /// Update federation settings
        /// </summary>
        [HttpPut]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(FederationSettingsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<FederationSettingsDto>> UpdateFederationSettings([FromBody] UpdateFederationSettingsRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid authentication token");
            }

            var settings = new FederationSettings
            {
                IsFederationEnabled = request.IsFederationEnabled,
                InstanceName = request.InstanceName,
                InstanceDescription = request.InstanceDescription,
                AdminContact = request.AdminContact,
                BaseUrl = request.BaseUrl,
                KeyRotationIntervalDays = request.KeyRotationIntervalDays,
                RequireHttps = request.RequireHttps,
                AllowSelfSignedCertificates = request.AllowSelfSignedCertificates,
                HandshakeTimeoutMinutes = request.HandshakeTimeoutMinutes,
                DefaultSyncIntervalMinutes = request.DefaultSyncIntervalMinutes,
                MaxConcurrentSyncs = request.MaxConcurrentSyncs,
                MaxModelsPerInstance = request.MaxModelsPerInstance,
                MaxModelFileSizeBytes = request.MaxModelFileSizeBytes,
                ModelCacheRetentionDays = request.ModelCacheRetentionDays,
                AutoAcceptHandshakes = request.AutoAcceptHandshakes,
                SharePublicModelsOnly = request.SharePublicModelsOnly,
                ShareFeaturedModelsOnly = request.ShareFeaturedModelsOnly,
                AllowedFileTypes = request.AllowedFileTypes ?? string.Empty,
                BlockedCategories = request.BlockedCategories ?? string.Empty,
                AllowNSFWContent = request.AllowNSFWContent,
                RequireApprovalForNewInstances = request.RequireApprovalForNewInstances,
                MaxRequestsPerMinute = request.MaxRequestsPerMinute,
                MaxDownloadsPerHour = request.MaxDownloadsPerHour,
                MaxBandwidthPerDay = request.MaxBandwidthPerDay,
                HeartbeatIntervalMinutes = request.HeartbeatIntervalMinutes,
                HealthCheckTimeoutSeconds = request.HealthCheckTimeoutSeconds,
                EnableMetrics = request.EnableMetrics,
                EnableDetailedLogging = request.EnableDetailedLogging,
                EnableDiscovery = request.EnableDiscovery,
                DiscoveryTags = request.DiscoveryTags,
                EnableAutomaticBackup = request.EnableAutomaticBackup,
                BackupRetentionDays = request.BackupRetentionDays
            };

            try
            {
                var updatedSettings = await _federationService.UpdateFederationSettingsAsync(settings, userId);
                
                var dto = new FederationSettingsDto
                {
                    Id = updatedSettings.Id,
                    IsFederationEnabled = updatedSettings.IsFederationEnabled,
                    InstanceName = updatedSettings.InstanceName,
                    InstanceDescription = updatedSettings.InstanceDescription,
                    AdminContact = updatedSettings.AdminContact,
                    BaseUrl = updatedSettings.BaseUrl,
                    KeyRotationIntervalDays = updatedSettings.KeyRotationIntervalDays,
                    LastKeyRotation = updatedSettings.LastKeyRotation,
                    RequireHttps = updatedSettings.RequireHttps,
                    AllowSelfSignedCertificates = updatedSettings.AllowSelfSignedCertificates,
                    HandshakeTimeoutMinutes = updatedSettings.HandshakeTimeoutMinutes,
                    DefaultSyncIntervalMinutes = updatedSettings.DefaultSyncIntervalMinutes,
                    MaxConcurrentSyncs = updatedSettings.MaxConcurrentSyncs,
                    MaxModelsPerInstance = updatedSettings.MaxModelsPerInstance,
                    MaxModelFileSizeBytes = updatedSettings.MaxModelFileSizeBytes,
                    ModelCacheRetentionDays = updatedSettings.ModelCacheRetentionDays,
                    AutoAcceptHandshakes = updatedSettings.AutoAcceptHandshakes,
                    SharePublicModelsOnly = updatedSettings.SharePublicModelsOnly,
                    ShareFeaturedModelsOnly = updatedSettings.ShareFeaturedModelsOnly,
                    AllowedFileTypes = updatedSettings.AllowedFileTypes,
                    BlockedCategories = updatedSettings.BlockedCategories,
                    AllowNSFWContent = updatedSettings.AllowNSFWContent,
                    RequireApprovalForNewInstances = updatedSettings.RequireApprovalForNewInstances,
                    MaxRequestsPerMinute = updatedSettings.MaxRequestsPerMinute,
                    MaxDownloadsPerHour = updatedSettings.MaxDownloadsPerHour,
                    MaxBandwidthPerDay = updatedSettings.MaxBandwidthPerDay,
                    HeartbeatIntervalMinutes = updatedSettings.HeartbeatIntervalMinutes,
                    HealthCheckTimeoutSeconds = updatedSettings.HealthCheckTimeoutSeconds,
                    EnableMetrics = updatedSettings.EnableMetrics,
                    EnableDetailedLogging = updatedSettings.EnableDetailedLogging,
                    FederationInviteUrl = updatedSettings.FederationInviteUrl,
                    InviteUrlGeneratedAt = updatedSettings.InviteUrlGeneratedAt,
                    InviteUrlExpiresAt = updatedSettings.InviteUrlExpiresAt,
                    EnableDiscovery = updatedSettings.EnableDiscovery,
                    DiscoveryTags = updatedSettings.DiscoveryTags,
                    EnableAutomaticBackup = updatedSettings.EnableAutomaticBackup,
                    BackupRetentionDays = updatedSettings.BackupRetentionDays,
                    LastBackupAt = updatedSettings.LastBackupAt,
                    FederationProtocolVersion = updatedSettings.FederationProtocolVersion,
                    SupportedVersions = updatedSettings.SupportedVersions,
                    TotalConnectedInstances = updatedSettings.TotalConnectedInstances,
                    TotalModelsShared = updatedSettings.TotalModelsShared,
                    TotalModelsReceived = updatedSettings.TotalModelsReceived,
                    TotalBytesTransferred = updatedSettings.TotalBytesTransferred,
                    LastStatsUpdate = updatedSettings.LastStatsUpdate
                };

                return Ok(dto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to update federation settings: {ex.Message}");
            }
        }
    }

    public class UpdateFederationSettingsRequest
    {
        [Required]
        public bool IsFederationEnabled { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string InstanceName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string InstanceDescription { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string AdminContact { get; set; } = string.Empty;
        
        [Required]
        [Url]
        public string BaseUrl { get; set; } = string.Empty;
        
        [Range(30, 365)]
        public int KeyRotationIntervalDays { get; set; } = 90;
        
        public bool RequireHttps { get; set; } = true;
        public bool AllowSelfSignedCertificates { get; set; } = false;
        
        [Range(1, 60)]
        public int HandshakeTimeoutMinutes { get; set; } = 5;
        
        [Range(15, 1440)]
        public int DefaultSyncIntervalMinutes { get; set; } = 60;
        
        [Range(1, 20)]
        public int MaxConcurrentSyncs { get; set; } = 5;
        
        [Range(100, 100000)]
        public int MaxModelsPerInstance { get; set; } = 10000;
        
        [Range(1024 * 1024, 1024L * 1024 * 1024 * 10)] // 1MB to 10GB
        public long MaxModelFileSizeBytes { get; set; } = 100L * 1024 * 1024;
        
        [Range(1, 365)]
        public int ModelCacheRetentionDays { get; set; } = 30;
        
        public bool AutoAcceptHandshakes { get; set; } = false;
        public bool SharePublicModelsOnly { get; set; } = true;
        public bool ShareFeaturedModelsOnly { get; set; } = false;
        
        public string? AllowedFileTypes { get; set; }
        public string? BlockedCategories { get; set; }
        public bool AllowNSFWContent { get; set; } = false;
        public bool RequireApprovalForNewInstances { get; set; } = true;
        
        [Range(10, 10000)]
        public int MaxRequestsPerMinute { get; set; } = 100;
        
        [Range(100, 100000)]
        public int MaxDownloadsPerHour { get; set; } = 1000;
        
        [Range(1024L * 1024 * 1024, 1024L * 1024 * 1024 * 1024)] // 1GB to 1TB
        public long MaxBandwidthPerDay { get; set; } = 10L * 1024 * 1024 * 1024;
        
        [Range(5, 120)]
        public int HeartbeatIntervalMinutes { get; set; } = 15;
        
        [Range(10, 300)]
        public int HealthCheckTimeoutSeconds { get; set; } = 30;
        
        public bool EnableMetrics { get; set; } = true;
        public bool EnableDetailedLogging { get; set; } = false;
        public bool EnableDiscovery { get; set; } = false;
        
        public string? DiscoveryTags { get; set; }
        public bool EnableAutomaticBackup { get; set; } = true;
        
        [Range(7, 365)]
        public int BackupRetentionDays { get; set; } = 30;
    }
} 