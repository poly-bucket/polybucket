using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.Services;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Domain
{
    public class DeleteAllFileSettingsService(
        IDeleteAllFileSettingsRepository repository,
        IAllowedFileTypesSyncService allowedFileTypesSyncService,
        ILogger<DeleteAllFileSettingsService> logger) : IDeleteAllFileSettingsService
    {
        private readonly IDeleteAllFileSettingsRepository _repository = repository;
        private readonly IAllowedFileTypesSyncService _allowedFileTypesSyncService = allowedFileTypesSyncService;
        private readonly ILogger<DeleteAllFileSettingsService> _logger = logger;

        public async Task<DeleteAllFileSettingsResponse> DeleteAllFileSettingsAsync(
            DeleteAllFileSettingsRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            if (string.IsNullOrEmpty(request.AdminPassword))
            {
                throw new UnauthorizedAccessException("Admin password is required");
            }

            _logger.LogWarning("Admin {AdminId} is attempting to delete ALL file type settings", userId);

            var deletedCount = await _repository.DeleteAllFileSettingsAndReturnCountAsync(userId, cancellationToken);

            try
            {
                await _allowedFileTypesSyncService.SyncFromFileTypeSettingsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "File types deleted but AllowedFileTypes sync failed");
            }

            if (deletedCount == 0)
            {
                return new DeleteAllFileSettingsResponse
                {
                    Success = true,
                    Message = "No file type settings found to delete",
                    DeletedCount = 0,
                    DeletedAt = DateTime.UtcNow
                };
            }

            _logger.LogCritical(
                "Admin {AdminId} has deleted ALL {FileTypeCount} file type settings from the system",
                userId,
                deletedCount);

            return new DeleteAllFileSettingsResponse
            {
                Success = true,
                Message = $"Successfully deleted {deletedCount} file type settings",
                DeletedCount = deletedCount,
                DeletedAt = DateTime.UtcNow
            };
        }
    }
}
