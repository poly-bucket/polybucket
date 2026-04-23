using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain;
using PolyBucket.Api.Common.Models.Enums;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain
{
    public class GetSiteModelSettingsQueryHandler(
        PolyBucketDbContext context,
        ILogger<GetSiteModelSettingsQueryHandler> logger) : IRequestHandler<GetSiteModelSettingsQuery, GetSiteModelSettingsResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<GetSiteModelSettingsQueryHandler> _logger = logger;

        public async Task<GetSiteModelSettingsResponse> Handle(GetSiteModelSettingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
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
