using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.Queries
{
    [ApiController]
    [Route("api/users/settings")]
    [Authorize]
    public class GetUserSettingsController : ControllerBase
    {
        private readonly GetUserSettingsQueryHandler _handler;
        private readonly ILogger<GetUserSettingsController> _logger;

        public GetUserSettingsController(GetUserSettingsQueryHandler handler, ILogger<GetUserSettingsController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<GetUserSettingsResponse>> GetUserSettings()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
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