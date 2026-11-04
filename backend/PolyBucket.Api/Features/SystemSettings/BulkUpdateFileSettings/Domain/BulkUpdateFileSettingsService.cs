using System.Security.Claims;
using PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Domain;

public class BulkUpdateFileSettingsService(
    IBulkUpdateFileSettingsRepository repository,
    IAllowedFileTypesSyncService allowedFileTypesSyncService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<BulkUpdateFileSettingsService> logger) : IBulkUpdateFileSettingsService
{
    private readonly IBulkUpdateFileSettingsRepository _repository = repository;
    private readonly IAllowedFileTypesSyncService _allowedFileTypesSyncService = allowedFileTypesSyncService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<BulkUpdateFileSettingsService> _logger = logger;

    public async Task<BulkUpdateFileSettingsResponse> BulkUpdateAsync(
        BulkUpdateFileSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = command.Ids.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return new BulkUpdateFileSettingsResponse
            {
                Success = false,
                Message = "At least one file type id is required.",
                FailedCount = 0,
                UpdatedCount = 0
            };
        }

        var userId = ResolveUserId();
        var fileTypes = await _repository.GetFileTypesByIdsAsync(distinctIds, cancellationToken);
        var foundIds = fileTypes.Select(ft => ft.Id).ToHashSet();
        var results = new List<BulkUpdateFileSettingsItemResult>();

        foreach (var id in distinctIds)
        {
            if (!foundIds.Contains(id))
            {
                results.Add(new BulkUpdateFileSettingsItemResult
                {
                    Id = id,
                    Success = false,
                    Message = "File type settings not found."
                });
            }
        }

        foreach (var fileType in fileTypes)
        {
            fileType.Enabled = command.Enabled;
            fileType.UpdatedAt = DateTime.UtcNow;
            if (userId.HasValue)
            {
                fileType.UpdatedById = userId.Value;
            }

            results.Add(new BulkUpdateFileSettingsItemResult
            {
                Id = fileType.Id,
                FileExtension = fileType.FileExtension,
                Success = true,
                Message = command.Enabled ? "Enabled" : "Disabled"
            });
        }

        await _repository.SaveChangesAsync(cancellationToken);

        try
        {
            await _allowedFileTypesSyncService.SyncFromFileTypeSettingsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File types updated but AllowedFileTypes sync failed");
        }

        var updatedCount = results.Count(r => r.Success);
        var failedCount = results.Count(r => !r.Success);

        return new BulkUpdateFileSettingsResponse
        {
            Success = failedCount == 0,
            Message = failedCount == 0
                ? "File type settings updated successfully"
                : "Some file type settings could not be updated",
            UpdatedCount = updatedCount,
            FailedCount = failedCount,
            Results = results
        };
    }

    private Guid? ResolveUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}
