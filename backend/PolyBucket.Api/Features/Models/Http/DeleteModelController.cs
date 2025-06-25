using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class DeleteModelController : ControllerBase
    {
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteModel(Guid id)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 