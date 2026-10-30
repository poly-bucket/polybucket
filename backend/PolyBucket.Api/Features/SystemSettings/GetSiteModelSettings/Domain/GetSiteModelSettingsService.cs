using PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain;
using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain
{
    public class GetSiteModelSettingsService(
        IGetSiteModelSettingsRepository repository,
        ILogger<GetSiteModelSettingsService> logger) : IGetSiteModelSettingsService
    {
        private readonly IGetSiteModelSettingsRepository _repository = repository;
        private readonly ILogger<GetSiteModelSettingsService> _logger = logger;

        public async Task<GetSiteModelSettingsResponse> GetSiteModelSettingsAsync(GetSiteModelSettingsQuery request, CancellationToken cancellationToken = default)
        {
            try
            {
                var systemSetup = await _repository.Context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
                if (systemSetup == null)
                {
                    return new GetSiteModelSettingsResponse
                    {
                        Success = false,
                        Message = "System setup not found. Please complete first-time setup first."
                    };
                }

                var settings = new SiteModelSettingsData
                {
                    MaxFileSizeBytes = systemSetup.MaxFileSizeBytes,
                    AllowedFileTypes = systemSetup.AllowedFileTypes,
                    MaxFilesPerUpload = systemSetup.MaxFilesPerUpload,
                    EnableFileCompression = systemSetup.EnableFileCompression,
                    AutoGeneratePreviews = systemSetup.AutoGeneratePreviews,
                    DefaultModelPrivacy = systemSetup.DefaultModelPrivacy.ToString(),
                    AutoApproveModels = systemSetup.AutoApproveModels,
                    RequireModeration = systemSetup.RequireModeration,
                    RequireLoginForUpload = systemSetup.RequireLoginForUpload,
                    AllowPublicBrowsing = systemSetup.AllowPublicBrowsing
                };

                return new GetSiteModelSettingsResponse
                {
                    Success = true,
                    Message = "Site model settings retrieved successfully",
                    Settings = settings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve site model settings");
                return new GetSiteModelSettingsResponse
                {
                    Success = false,
                    Message = "Failed to retrieve site model settings. Please try again."
                };
            }
        }
    }
}
