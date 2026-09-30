using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.Authorization;
using PolyBucket.Api.Features.Collections.UpdateCollection.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.UpdateCollection.Http
{
    [ApiController]
    [Route("api/collections")]
    [Authorize]
    public class UpdateCollectionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Updates the name, description, or visibility of one of the caller's collections.
        /// </summary>
        /// <response code="200">The updated collection.</response>
        /// <response code="400">The route ID does not match the body.</response>
        /// <response code="403">The user's email address must be verified first.</response>
        [HttpPut("{id}")]
        [RequireVerifiedEmail]
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