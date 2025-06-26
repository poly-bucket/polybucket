using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.Queries;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
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