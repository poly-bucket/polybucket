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
    public class CreateCollectionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CreateCollectionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionCommand command)
        {
            var collection = await _mediator.Send(command);
            return CreatedAtAction(nameof(collection), new { id = collection.Id }, collection);
        }
    }
} 