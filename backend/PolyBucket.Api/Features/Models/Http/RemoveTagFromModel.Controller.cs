using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class RemoveTagFromModelController : ControllerBase
    {
        [HttpDelete("{id}/tags/{tagId}")]
        public async Task<ActionResult> RemoveTagFromModel(Guid id, Guid tagId)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 