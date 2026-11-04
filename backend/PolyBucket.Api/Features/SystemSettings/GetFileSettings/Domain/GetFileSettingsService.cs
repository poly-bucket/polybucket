using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

public interface IGetFileSettingsService
{
    Task<GetFileSettingsResponse> GetFileSettingsAsync(
        GetFileSettingsQueryParams query,
        CancellationToken cancellationToken = default);
}

public class GetFileSettingsService(
    IGetFileSettingsRepository repository,
    ILogger<GetFileSettingsService> logger) : IGetFileSettingsService
{
    private readonly IGetFileSettingsRepository _repository = repository;
    private readonly ILogger<GetFileSettingsService> _logger = logger;

    public async Task<GetFileSettingsResponse> GetFileSettingsAsync(
        GetFileSettingsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.GetFileSettingsAsync(query, cancellationToken);
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
