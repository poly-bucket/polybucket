using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.RemoveModelFromCollection.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class RemoveModelFromCollectionController : ControllerBase
    {
        [HttpDelete("{id}/collections/{collectionId}")]
        public async Task<ActionResult> RemoveModelFromCollection(Guid id, Guid collectionId)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
}
