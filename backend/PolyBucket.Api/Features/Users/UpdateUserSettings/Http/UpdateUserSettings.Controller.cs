using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Handlers;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Http
{
    [ApiController]
    [Route("api/users/settings")]
    [Authorize]
    public class UpdateUserSettingsController(UpdateUserSettingsCommandHandler handler, ILogger<UpdateUserSettingsController> logger) : ControllerBase
    {
        private readonly UpdateUserSettingsCommandHandler _handler = handler;
        private readonly ILogger<UpdateUserSettingsController> _logger = logger;

        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> UpdateUserSettings([FromBody] UpdateUserSettingsRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid authentication token");
                }

                var command = new UpdateUserSettingsCommand
                {
                    UserId = userId,
                    Language = request.Language,
                    Theme = request.Theme,
                    EmailNotifications = request.EmailNotifications,
                    DefaultPrinterId = !string.IsNullOrEmpty(request.DefaultPrinterId) && Guid.TryParse(request.DefaultPrinterId, out var printerId) ? printerId : null,
                    MeasurementSystem = request.MeasurementSystem,
                    TimeZone = request.TimeZone,
                    AutoRotateModels = request.AutoRotateModels,
                    DashboardViewType = request.DashboardViewType,
                    CardSize = request.CardSize,
                    CardSpacing = request.CardSpacing,
                    GridColumns = request.GridColumns,
                    CustomSettings = request.CustomSettings
                };

                await _handler.ExecuteAsync(command);
                _logger.LogInformation($"{nameof(UpdateUserSettingsController)}: User settings updated successfully");
                return Ok(new { message = "User settings updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateUserSettingsController)}: An error occurred while updating user settings");
                return StatusCode(500, new { message = "An error occurred while updating user settings" });
            }
        }
    }

    public class UpdateUserSettingsRequest
    {
        public string? Language { get; set; }
        public string? Theme { get; set; }
        public bool? EmailNotifications { get; set; }
        public string? DefaultPrinterId { get; set; }
        public string? MeasurementSystem { get; set; }
        public string? TimeZone { get; set; }
        public bool? AutoRotateModels { get; set; }
        public string? DashboardViewType { get; set; }
        public string? CardSize { get; set; }
        public string? CardSpacing { get; set; }
        public int? GridColumns { get; set; }
        public Dictionary<string, string>? CustomSettings { get; set; }
    }
} 