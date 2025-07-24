using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.DeleteCollection.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.DeleteCollection.Http
{
    [ApiController]
    [Route("api/collections")]
    [Authorize]
    public class DeleteCollectionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollection(Guid id)
        {
            var command = new DeleteCollectionCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
} 