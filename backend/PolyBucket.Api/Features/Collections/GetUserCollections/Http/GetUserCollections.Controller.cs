using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.GetUserCollections.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Http
{
    [ApiController]
    [Route("api/collections")]
    [Authorize]
    public class GetUserCollectionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GetUserCollectionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetUserCollections()
        {
            var query = new GetUserCollectionsQuery();
            var collections = await _mediator.Send(query);
            return Ok(collections);
        }
    }
} 