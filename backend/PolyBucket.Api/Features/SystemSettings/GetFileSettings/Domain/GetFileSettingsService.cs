using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

public class GetFileSettingsService(
    IGetFileSettingsRepository repository,
    ILogger<GetFileSettingsService> logger) : IGetFileSettingsService
{
    private readonly IGetFileSettingsRepository _repository = repository;
    private readonly ILogger<GetFileSettingsService> _logger = logger;

    public async Task<GetFileSettingsResponse> GetFileSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var fileTypes = await _repository.GetFileTypesAsync(cancellationToken);
            return new GetFileSettingsResponse
            {
                Success = true,
                Message = "File type settings retrieved successfully",
                FileTypes = fileTypes.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve file type settings");
            return new GetFileSettingsResponse
            {
                Success = false,
                Message = "Failed to retrieve file type settings. Please try again."
            };
        }
    }
}
