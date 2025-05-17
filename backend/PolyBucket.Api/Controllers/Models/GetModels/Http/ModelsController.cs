using Api.Controllers.Models.GetModelById.Domain;
using Api.Controllers.Models.GetModels.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Models.GetModels.Http
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class ModelsController(IGetModelsService getModelsService, IGetModelByIdService getModelByIdService, ILogger<ModelsController> logger) : ControllerBase
    {
        private readonly IGetModelsService _getModelsService = getModelsService;
        private readonly IGetModelByIdService _getModelByIdService = getModelByIdService;
        private readonly ILogger<ModelsController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<GetModelsResponse>> GetModels([FromQuery] GetModelsRequest request)
        {
            try
            {
                var response = await _getModelsService.ExecuteAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving models");
                return StatusCode(500, new { message = "An error occurred while retrieving models" });
            }
        }
    }
}