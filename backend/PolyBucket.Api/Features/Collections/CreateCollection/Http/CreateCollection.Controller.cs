using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.CreateCollection.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Http
{
    [ApiController]
    [Route("api/collections")]
    [Authorize]
    public class CreateCollectionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Creates a new collection for the authenticated user.
        /// </summary>
        /// <param name="command">Collection creation payload.</param>
        /// <response code="200">Collection created successfully.</response>
        /// <response code="400">Request payload is invalid.</response>
        [HttpPost]
        [ProducesResponseType(typeof(PolyBucket.Api.Features.Collections.Domain.Collection), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionCommand command)
        {
            var collection = await _mediator.Send(command);
            return Ok(collection);
        }
    }
}