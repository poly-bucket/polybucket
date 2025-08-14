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

        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionCommand command)
        {
            var collection = await _mediator.Send(command);
            return Created($"/api/collections/{collection.Id}", collection);
        }
    }
} 