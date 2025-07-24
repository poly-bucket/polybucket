using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.AccessCollection.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.AccessCollection.Http
{
    [ApiController]
    [Route("api/collections")]
    public class AccessCollectionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost("{id}/access")]
        public async Task<IActionResult> AccessCollection(Guid id, [FromBody] AccessCollectionCommand command)
        {
            if (id != command.CollectionId)
            {
                return BadRequest("ID in the route must match ID in the body.");
            }

            var response = await _mediator.Send(command);

            if (!response.Success)
            {
                if (response.RequiresPassword)
                {
                    return Unauthorized(new { message = response.Message, requiresPassword = true });
                }
                return NotFound(new { message = response.Message });
            }

            return Ok(response.Collection);
        }
    }
} 