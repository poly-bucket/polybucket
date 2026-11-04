namespace PolyBucket.Api.Features.SystemSettings.SyncAllowedFileTypes.Domain;

public class SyncAllowedFileTypesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string DerivedAllowedFileTypes { get; set; } = string.Empty;
}

public interface ISyncAllowedFileTypesService
{
    Task<SyncAllowedFileTypesResponse> SyncAsync(CancellationToken cancellationToken = default);
}

public class SyncAllowedFileTypesService(
    Services.IAllowedFileTypesSyncService allowedFileTypesSyncService,
    ILogger<SyncAllowedFileTypesService> logger) : ISyncAllowedFileTypesService
{
    private readonly Services.IAllowedFileTypesSyncService _allowedFileTypesSyncService = allowedFileTypesSyncService;
    private readonly ILogger<SyncAllowedFileTypesService> _logger = logger;

    public async Task<SyncAllowedFileTypesResponse> SyncAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var derived = await _allowedFileTypesSyncService.SyncFromFileTypeSettingsAsync(cancellationToken);
            return new SyncAllowedFileTypesResponse
            {
                Success = true,
                Message = "Allowed file types synced from file type settings",
                DerivedAllowedFileTypes = derived
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync allowed file types");
            return new SyncAllowedFileTypesResponse
            {
                Success = false,
                Message = "Failed to sync allowed file types. Please try again."
            };
        }
    }
}
