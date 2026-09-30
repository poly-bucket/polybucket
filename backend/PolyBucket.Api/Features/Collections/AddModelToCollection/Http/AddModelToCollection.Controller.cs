using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.Authorization;
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

        /// <summary>
        /// Adds a model to one of the caller's collections.
        /// </summary>
        /// <response code="204">The model was added.</response>
        /// <response code="403">The user's email address must be verified first.</response>
        [HttpPost("{collectionId}/models/{modelId}")]
        [RequireVerifiedEmail]
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