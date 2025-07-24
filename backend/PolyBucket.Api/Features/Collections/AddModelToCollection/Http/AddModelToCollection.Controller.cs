using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.AddModelToCollection.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.AddModelToCollection.Http
{
    [ApiController]
    [Route("api/collections")]
    [Authorize]
    public class AddModelToCollectionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost("{collectionId}/models/{modelId}")]
        public async Task<IActionResult> AddModelToCollection(Guid collectionId, Guid modelId)
        {
            var command = new AddModelToCollectionCommand
            {
                CollectionId = collectionId,
                ModelId = modelId
            };

            await _mediator.Send(command);
            return NoContent();
        }
    }
} 