using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.AddModelToCollection.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class AddModelToCollectionController : ControllerBase
    {
        [HttpPost("{id}/collections/{collectionId}")]
        public async Task<ActionResult> AddModelToCollection(Guid id, Guid collectionId)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
}
