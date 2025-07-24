using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using PolyBucket.Api.Features.Users.Queries;

namespace PolyBucket.Api.Features.Users.Queries
{
    [ApiController]
    [Route("api/users/settings")]
    [Authorize]
    public class GetUserSettingsController(GetUserSettingsQueryHandler handler, ILogger<GetUserSettingsController> logger) : ControllerBase
    {
        private readonly GetUserSettingsQueryHandler _handler = handler;
        private readonly ILogger<GetUserSettingsController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<GetUserSettingsResponse>> GetUserSettings()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized();
                }

                var request = new GetUserSettingsRequest { UserId = userGuid };
                var settings = await _handler.ExecuteAsync(request);
                return Ok(settings);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user settings");
                return StatusCode(500, new { message = "An error occurred while retrieving user settings" });
            }
        }
    }
} 