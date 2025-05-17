using Api.Controllers.Models.GetModelById.Domain;
using Api.Controllers.Models.GetModels.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Models.Http;

[ApiController]
[Route("api/[controller]")]
public partial class ModelsController : ControllerBase
{
    private readonly IGetModelsService _getModelsService;
    private readonly IGetModelByIdService _getModelByIdService;
    private readonly ILogger<ModelsController> _logger;

    public ModelsController(
        IGetModelsService getModelsService,
        IGetModelByIdService getModelByIdService,
        ILogger<ModelsController> logger)
    {
        _getModelsService = getModelsService;
        _getModelByIdService = getModelByIdService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetModelByIdResponse>> GetModel(Guid id)
    {
        try
        {
            var request = new GetModelByIdRequest { Id = id };
            var response = await _getModelByIdService.ExecuteAsync(request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model with ID {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the model" });
        }
    }
}