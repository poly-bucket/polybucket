using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Domain;

namespace PolyBucket.Api.Features.Models.Http
{
    [ApiController]
    [Route("api/models")]
    public class GetModelByIdController : ControllerBase
    {
        private readonly IModelService _modelService;

        public GetModelByIdController(IModelService modelService)
        {
            _modelService = modelService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Model>> GetModelById(Guid id)
        {
            // TODO: Add moderation checks back
            var model = await _modelService.GetModelByIdAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            // TODO: Record view interaction
            return Ok(model);
        }
    }
} 