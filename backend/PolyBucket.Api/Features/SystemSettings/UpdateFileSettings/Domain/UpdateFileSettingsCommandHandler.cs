using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Domain;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Domain
{
    public class UpdateFileSettingsCommandHandler(
        PolyBucketDbContext context,
        ILogger<UpdateFileSettingsCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<UpdateFileSettingsCommand, UpdateFileSettingsResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<UpdateFileSettingsCommandHandler> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<UpdateFileSettingsResponse> Handle(UpdateFileSettingsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var fileType = await _context.FileTypeSettings
                    .FirstOrDefaultAsync(ft => ft.Id == request.Id && !ft.IsDeleted, cancellationToken);
                
                if (fileType == null)
                {
                    return new UpdateFileSettingsResponse
                    {
                        Success = false,
                        Message = "File type settings not found."
                    };
                }

                // Update file type settings
                fileType.FileExtension = request.FileExtension;
                fileType.Enabled = request.Enabled;
                fileType.MaxFileSizeBytes = request.MaxFileSizeBytes;
                fileType.MaxPerUpload = request.MaxPerUpload;
                fileType.DisplayName = request.DisplayName;
                fileType.Description = request.Description;
                fileType.MimeType = request.MimeType;
                fileType.RequiresPreview = request.RequiresPreview;
                fileType.IsCompressible = request.IsCompressible;
                fileType.Category = request.Category;
                fileType.Priority = request.Priority;
                fileType.IsDefault = request.IsDefault;
                
                // Update audit fields
                fileType.UpdatedAt = DateTime.UtcNow;
                
                // Get current user ID if available
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    fileType.UpdatedById = userId;
                }

                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("File type settings updated successfully by user {UserId} for file type {FileExtension}", 
                    userIdClaim?.Value ?? "unknown", request.FileExtension);

                return new UpdateFileSettingsResponse
                {
                    Success = true,
                    Message = "File type settings updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update file type settings");
                return new UpdateFileSettingsResponse
                {
                    Success = false,
                    Message = "Failed to update file type settings. Please try again."
                };
            }
        }
    }
}
