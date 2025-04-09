using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GetModels.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModels.Http
{
    [ApiController]
    [Route("api/models")]
    public class GetModelsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GetModelsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<GetModelsResponse>> GetModels([FromQuery] GetModelsQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
} 