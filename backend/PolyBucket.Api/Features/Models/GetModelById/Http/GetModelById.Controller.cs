using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GetModelById.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelById.Http
{
    [ApiController]
    [Route("api/models")]
    public class GetModelByIdController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GetModelByIdController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetModelByIdResponse>> GetModel(Guid id)
        {
            var query = new GetModelByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
} 