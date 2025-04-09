using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Http
{
    [Authorize]
    [ApiController]
    [Route("api/moderation/models")]
    public class UpdateModerationSettingsController : ControllerBase
    {
        [HttpPut("settings")]
        public async Task<ActionResult> UpdateModerationSettings(/*[FromBody] ModerationSettings settings*/)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 