using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.FavoriteCollection.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.FavoriteCollection.Http
{
    /// <summary>
    /// Controller for favoriting/unfavoriting collections
    /// </summary>
    [ApiController]
    [Route("api/collections")]
    [Authorize]
    public class FavoriteCollectionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FavoriteCollectionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Toggle favorite status of a collection
        /// </summary>
        /// <param name="id">Collection ID</param>
        /// <param name="command">Favorite command</param>
        /// <returns>Success status and message</returns>
        /// <response code="200">Collection favorite status updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized to modify this collection</response>
        /// <response code="404">Collection not found</response>
        [HttpPost("{id}/favorite")]
        public async Task<IActionResult> ToggleFavorite(Guid id, [FromBody] FavoriteCollectionCommand command)
        {
            if (id != command.CollectionId)
            {
                return BadRequest("ID in the route must match ID in the body.");
            }

            var response = await _mediator.Send(command);

            if (!response.Success)
            {
                if (response.Message.Contains("not authenticated"))
                    return Unauthorized(new { message = response.Message });
                if (response.Message.Contains("not found"))
                    return NotFound(new { message = response.Message });
                if (response.Message.Contains("only favorite your own"))
                    return Forbid();
                
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
    }
}
