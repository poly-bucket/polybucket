using System.Security.Claims;
using PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Domain;

public class UpdateFileSettingsService(
    IUpdateFileSettingsRepository repository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UpdateFileSettingsService> logger) : IUpdateFileSettingsService
{
    private readonly IUpdateFileSettingsRepository _repository = repository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<UpdateFileSettingsService> _logger = logger;

    public async Task<UpdateFileSettingsResponse> UpdateFileSettingsAsync(UpdateFileSettingsCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileType = await _repository.GetFileTypeAsync(request.Id, cancellationToken);

            if (fileType == null)
            {
                return new UpdateFileSettingsResponse
                {
                    Success = false,
                    Message = "File type settings not found."
                };
            }

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
            fileType.UpdatedAt = DateTime.UtcNow;

            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                fileType.UpdatedById = userId;
            }

            await _repository.SaveChangesAsync(cancellationToken);

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
