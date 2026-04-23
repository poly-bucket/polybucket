using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Http;

[ApiController]
[Route("api/users/settings")]
[Authorize]
public class UpdateUserSettingsController : ControllerBase
{
    private readonly IUpdateUserSettingsService _updateUserSettingsService;
    private readonly ILogger<UpdateUserSettingsController> _logger;

    public UpdateUserSettingsController(
        IUpdateUserSettingsService updateUserSettingsService,
        ILogger<UpdateUserSettingsController> logger)
    {
        _updateUserSettingsService = updateUserSettingsService;
        _logger = logger;
    }

    [HttpPut]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> UpdateUserSettings(
        [FromBody] UpdateUserSettingsRequest request,
        CancellationToken cancellationToken = default)
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

            await _updateUserSettingsService.UpdateAsync(command, cancellationToken);
            _logger.LogInformation("{Controller}: User settings updated successfully", nameof(UpdateUserSettingsController));
            return Ok(new { message = "User settings updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Controller}: An error occurred while updating user settings", nameof(UpdateUserSettingsController));
            return StatusCode(500, new { message = "An error occurred while updating user settings" });
        }
    }
}
