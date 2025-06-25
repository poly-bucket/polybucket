using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Features.Users.Commands
{
    [ApiController]
    [Route("api/users/settings")]
    [Authorize]
    public class UpdateUserSettingsCommandController : ControllerBase
    {
        private readonly UpdateUserSettingsCommandHandler _handler;
        private readonly ILogger<UpdateUserSettingsCommandController> _logger;

        public UpdateUserSettingsCommandController(UpdateUserSettingsCommandHandler handler, ILogger<UpdateUserSettingsCommandController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserSettings([FromBody] UpdateUserSettingsRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized();
                }

                request.UserId = userGuid;
                await _handler.ExecuteAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user settings");
                return StatusCode(500, new { message = "An error occurred while updating user settings" });
            }
        }
    }
} 