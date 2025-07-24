using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Http
{
    [ApiController]
    [Route("api/collections")]
    [Authorize]
    public class RemoveModelFromCollectionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpDelete("{collectionId}/models/{modelId}")]
        public async Task<IActionResult> RemoveModelFromCollection(Guid collectionId, Guid modelId)
        {
            var command = new RemoveModelFromCollectionCommand
            {
                CollectionId = collectionId,
                ModelId = modelId
            };

            await _mediator.Send(command);
            return NoContent();
        }
    }
} 