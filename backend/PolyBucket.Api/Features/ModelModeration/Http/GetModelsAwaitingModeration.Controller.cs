using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Http
{
    [Authorize]
    [ApiController]
    [Route("api/moderation/models")]
    public class GetModelsAwaitingModerationController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetModelsAwaitingModeration([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 