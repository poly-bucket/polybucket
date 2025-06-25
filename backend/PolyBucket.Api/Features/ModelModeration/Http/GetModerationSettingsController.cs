using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Http
{
    [Authorize]
    [ApiController]
    [Route("api/moderation/models")]
    public class GetModerationSettingsController : ControllerBase
    {
        [HttpGet("settings")]
        public async Task<ActionResult> GetModerationSettings()
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 