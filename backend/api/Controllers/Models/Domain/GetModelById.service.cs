using Api.Controllers.Models.Persistance;
using Microsoft.Extensions.Logging;

namespace Api.Controllers.Models.Domain;

public interface IGetModelByIdService
{
    Task<GetModelByIdResponse> ExecuteAsync(GetModelByIdRequest request);
}

public class GetModelByIdService : IGetModelByIdService
{
    private readonly IGetModelByIdDataAccess _dataAccess;
    private readonly ILogger<GetModelByIdService> _logger;

    public GetModelByIdService(
        IGetModelByIdDataAccess dataAccess,
        ILogger<GetModelByIdService> logger)
    {
        _dataAccess = dataAccess;
        _logger = logger;
    }

    public async Task<GetModelByIdResponse> ExecuteAsync(GetModelByIdRequest request)
    {
        try
        {
            var model = await _dataAccess.GetModelByIdAsync(request.Id);
            
            if (model == null)
            {
                throw new KeyNotFoundException($"Model with ID {request.Id} not found");
            }

            return new GetModelByIdResponse
            {
                Model = model
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model with ID {Id}", request.Id);
            throw;
        }
    }
} 