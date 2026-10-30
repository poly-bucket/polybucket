using PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models.Enums;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Domain
{
    public class UpdateSiteModelSettingsService(
        IUpdateSiteModelSettingsRepository repository,
        ILogger<UpdateSiteModelSettingsService> logger,
        IHttpContextAccessor httpContextAccessor,
        ISitePrivacyState sitePrivacyState) : IUpdateSiteModelSettingsService
    {
        private readonly IUpdateSiteModelSettingsRepository _repository = repository;
        private readonly ILogger<UpdateSiteModelSettingsService> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ISitePrivacyState _sitePrivacyState = sitePrivacyState;

        public async Task<UpdateSiteModelSettingsResponse> UpdateSiteModelSettingsAsync(UpdateSiteModelSettingsCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                var systemSetup = await _repository.Context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
                if (systemSetup == null)
                {
                    return new UpdateSiteModelSettingsResponse
                    {
                        Success = false,
                        Message = "System setup not found. Please complete first-time setup first."
                    };
                }

                // Parse and validate the privacy settings enum
                if (!Enum.TryParse<PrivacySettings>(request.DefaultModelPrivacy, true, out var privacySettings))
                {
                    return new UpdateSiteModelSettingsResponse
                    {
                        Success = false,
                        Message = "Invalid default model privacy value"
                    };
                }

                // Update file upload settings
                systemSetup.MaxFileSizeBytes = request.MaxFileSizeBytes;
                systemSetup.AllowedFileTypes = request.AllowedFileTypes;
                systemSetup.MaxFilesPerUpload = request.MaxFilesPerUpload;
                systemSetup.EnableFileCompression = request.EnableFileCompression;
                systemSetup.AutoGeneratePreviews = request.AutoGeneratePreviews;
                
                // Update default model settings
                systemSetup.DefaultModelPrivacy = privacySettings;
                systemSetup.AutoApproveModels = request.AutoApproveModels;
                systemSetup.RequireModeration = request.RequireModeration;
                
                // Update upload behavior
                systemSetup.RequireLoginForUpload = request.RequireLoginForUpload;
                systemSetup.AllowPublicBrowsing = request.AllowPublicBrowsing;
                
                // Update timestamps
                systemSetup.UpdatedAt = DateTime.UtcNow;
                
                // Get current user ID if available
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    systemSetup.UpdatedById = userId;
                }

                await _repository.Context.SaveChangesAsync(cancellationToken);
                _sitePrivacyState.Invalidate();
                
                _logger.LogInformation("Site model settings updated successfully by user {UserId}", 
                    userIdClaim?.Value ?? "unknown");

                return new UpdateSiteModelSettingsResponse
                {
                    Success = true,
                    Message = "Site model settings updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update site model settings");
                return new UpdateSiteModelSettingsResponse
                {
                    Success = false,
                    Message = "Failed to update site model settings. Please try again."
                };
            }
        }
    }
}
