using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Http
{
    [Authorize]
    [ApiController]
    [Route("api/moderation/models")]
    public class RejectModelController : ControllerBase
    {
        [HttpPost("{id}/reject")]
        public async Task<ActionResult> RejectModel(Guid id/*, [FromBody] ModelRejectRequest request*/)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 