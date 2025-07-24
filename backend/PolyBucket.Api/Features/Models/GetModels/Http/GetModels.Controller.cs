using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GetModels.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModels.Http
{
    [ApiController]
    [Route("api/models")]
    public class GetModelsController(IMediator mediator, ILogger<GetModelsController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<GetModelsController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<GetModelsResponse>> GetModels([FromQuery] GetModelsQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
} 