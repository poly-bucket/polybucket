using Api.Controllers.Models.Persistance;
using Microsoft.Extensions.Logging;

namespace Api.Controllers.Models.Domain;

public interface IGetModelsService
{
    Task<GetModelsResponse> ExecuteAsync(GetModelsRequest request);
}

public class GetModelsService : IGetModelsService
{
    private readonly IGetModelsDataAccess _dataAccess;
    private readonly ILogger<GetModelsService> _logger;

    public GetModelsService(
        IGetModelsDataAccess dataAccess,
        ILogger<GetModelsService> logger)
    {
        _dataAccess = dataAccess;
        _logger = logger;
    }

    public async Task<GetModelsResponse> ExecuteAsync(GetModelsRequest request)
    {
        try
        {
            var (models, totalCount) = await _dataAccess.GetModelsAsync(request.Page, request.Take);
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.Take);

            return new GetModelsResponse
            {
                Models = models,
                TotalCount = totalCount,
                Page = request.Page,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting models for page {Page}", request.Page);
            throw;
        }
    }
} 