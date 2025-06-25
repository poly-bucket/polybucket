using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class AddTagToModelController : ControllerBase
    {
        [HttpPost("{id}/tags")]
        public async Task<ActionResult> AddTagToModel(Guid id, [FromBody] string tagName)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 