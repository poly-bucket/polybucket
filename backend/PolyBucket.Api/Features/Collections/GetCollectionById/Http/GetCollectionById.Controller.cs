using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.GetCollectionById.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Http
{
    [ApiController]
    [Route("api/collections")]
    public class GetCollectionByIdController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollectionById(Guid id)
        {
            var query = new GetCollectionByIdQuery { Id = id };
            var collection = await _mediator.Send(query);

            if (collection == null)
            {
                return NotFound();
            }

            return Ok(collection);
        }
    }
} 