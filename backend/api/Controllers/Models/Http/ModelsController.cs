using Conductors.Models;
using Core.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Models.Http
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelsController : ControllerBase
    {
        private readonly IModelConductor _modelConductor;
        private readonly ILogger<ModelsController> _logger;

        public ModelsController(IModelConductor modelConductor, ILogger<ModelsController> logger)
        {
            _modelConductor = modelConductor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model>>> GetModels()
        {
            try
            {
                var models = await _modelConductor.GetModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving models");
                return StatusCode(500, new { message = "An error occurred while retrieving models" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Model>> GetModel(int id)
        {
            try
            {
                var model = await _modelConductor.GetModelByIdAsync(id);

                if (model is null)
                {
                    _logger.LogWarning("Model with ID {Id} not found", id);
                    return NotFound(new { message = $"Model with ID {id} not found" });
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving model with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the model" });
            }
        }
    }
}