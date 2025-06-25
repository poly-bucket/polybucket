using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Http
{
    [Authorize]
    [ApiController]
    [Route("api/moderation/models")]
    public class ApproveModelController : ControllerBase
    {
        [HttpPost("{id}/approve")]
        public async Task<ActionResult> ApproveModel(Guid id)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 