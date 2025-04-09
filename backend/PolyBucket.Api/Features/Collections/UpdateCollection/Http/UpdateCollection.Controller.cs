using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.UpdateCollection.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.UpdateCollection.Http
{
    [ApiController]
    [Route("api/collections")]
    [Authorize]
    public class UpdateCollectionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UpdateCollectionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(Guid id, [FromBody] UpdateCollectionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in the route must match ID in the body.");
            }

            var collection = await _mediator.Send(command);
            return Ok(collection);
        }
    }
} 