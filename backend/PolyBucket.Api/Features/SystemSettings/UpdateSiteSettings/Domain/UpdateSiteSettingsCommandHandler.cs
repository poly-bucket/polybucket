using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Domain
{
    public class UpdateSiteSettingsCommandHandler(
        PolyBucketDbContext context,
        ILogger<UpdateSiteSettingsCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<UpdateSiteSettingsCommand, UpdateSiteSettingsResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<UpdateSiteSettingsCommandHandler> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<UpdateSiteSettingsResponse> Handle(UpdateSiteSettingsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
                if (systemSetup == null)
                {
                    // Get current user ID for creation
                    var createUserIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                    var createUserId = Guid.Empty;
                    if (createUserIdClaim != null && Guid.TryParse(createUserIdClaim.Value, out var parsedUserId))
                    {
                        createUserId = parsedUserId;
                    }

                    systemSetup = new SystemSetup
                    {
                        Id = Guid.NewGuid(),
                        IsFirstTimeSetup = true,
                        IsAdminConfigured = false,
                        IsSiteConfigured = false,
                        IsEmailConfigured = false,
                        IsModerationConfigured = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = createUserId,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedById = createUserId
                    };
                    
                    _context.SystemSetups.Add(systemSetup);
                }

                // Update site settings
                systemSetup.SiteName = request.SiteName;
                systemSetup.SiteDescription = request.SiteDescription;
                systemSetup.ContactEmail = request.ContactEmail;
                systemSetup.AllowPublicBrowsing = request.AllowPublicBrowsing;
                systemSetup.RequireLoginForUpload = request.RequireLoginForUpload;
                systemSetup.AllowUserRegistration = request.AllowUserRegistration;
                systemSetup.RequireEmailVerification = request.RequireEmailVerification;
                
                // File upload settings
                systemSetup.MaxFileSizeBytes = request.MaxFileSizeBytes;
                systemSetup.AllowedFileTypes = request.AllowedFileTypes;
                systemSetup.MaxFilesPerUpload = request.MaxFilesPerUpload;
                systemSetup.EnableFileCompression = request.EnableFileCompression;
                systemSetup.AutoGeneratePreviews = request.AutoGeneratePreviews;
                
                // Default model settings
                systemSetup.DefaultModelPrivacy = request.DefaultModelPrivacy;
                systemSetup.AutoApproveModels = request.AutoApproveModels;
                systemSetup.RequireModeration = request.RequireModeration;
                
                // Role settings
                systemSetup.DefaultUserRole = request.DefaultUserRole;
                systemSetup.AllowCustomRoles = request.AllowCustomRoles;
                
                // Security settings
                systemSetup.MaxFailedLoginAttempts = request.MaxFailedLoginAttempts;
                systemSetup.LockoutDurationMinutes = request.LockoutDurationMinutes;
                systemSetup.RequireStrongPasswords = request.RequireStrongPasswords;
                systemSetup.PasswordMinLength = request.PasswordMinLength;
                
                // UI settings
                systemSetup.DefaultTheme = request.DefaultTheme;
                systemSetup.DefaultLanguage = request.DefaultLanguage;
                systemSetup.ShowAdvancedOptions = request.ShowAdvancedOptions;
                
                // Federation settings
                systemSetup.EnableFederation = request.EnableFederation;
                systemSetup.InstanceName = request.InstanceName;
                systemSetup.InstanceDescription = request.InstanceDescription;
                systemSetup.AdminContact = request.AdminContact ?? string.Empty;
                
                // Mark site as configured
                systemSetup.IsSiteConfigured = true;
                systemSetup.UpdatedAt = DateTime.UtcNow;
                
                // Get current user ID if available
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    systemSetup.UpdatedById = userId;
                }
                else
                {
                    // Use a default system user ID when no user context is available
                    systemSetup.UpdatedById = Guid.Empty;
                }

                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Site settings updated successfully by user {UserId}", 
                    userIdClaim?.Value ?? "unknown");

                return new UpdateSiteSettingsResponse
                {
                    Success = true,
                    Message = "Site settings updated successfully",
                    NextStep = "email"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update site settings");
                return new UpdateSiteSettingsResponse
                {
                    Success = false,
                    Message = "Failed to update site settings. Please try again.",
                    NextStep = "site"
                };
            }
        }
    }
} 