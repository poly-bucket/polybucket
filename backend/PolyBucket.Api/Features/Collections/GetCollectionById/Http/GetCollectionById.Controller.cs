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

        /// <summary>
        /// Gets a collection by id with owner summary and model thumbnails.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetCollectionByIdResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCollectionByIdResponseDto>> GetCollectionById(Guid id)
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