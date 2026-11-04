using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Http
{
    [ApiController]
    [Route("api/system-settings/file-settings")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    public class DeleteAllFileSettingsController(
        IDeleteAllFileSettingsService deleteAllFileSettingsService,
        ILogger<DeleteAllFileSettingsController> logger) : ControllerBase
    {
        private readonly IDeleteAllFileSettingsService _deleteAllFileSettingsService = deleteAllFileSettingsService;
        private readonly ILogger<DeleteAllFileSettingsController> _logger = logger;

        /// <summary>
        /// Delete ALL file type settings from the system (DANGEROUS OPERATION - Admin only)
        /// </summary>
        /// <param name="request">Delete request with admin password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the deletion operation</returns>
        [HttpDelete("delete-all")]
        [ProducesResponseType(typeof(DeleteAllFileSettingsResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DeleteAllFileSettingsResponse>> DeleteAllFileSettings(
            [FromBody] DeleteAllFileSettingsRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogWarning(
                    "Admin {AdminId} is attempting to delete ALL file type settings",
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var result = await _deleteAllFileSettingsService.DeleteAllFileSettingsAsync(
                    request,
                    User,
                    cancellationToken);

                if (result.Success)
                {
                    _logger.LogCritical(
                        "CRITICAL: All file type settings deleted by admin {AdminId}. Deleted count: {DeletedCount}",
                        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                        result.DeletedCount);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to delete all file type settings: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting all file type settings");
                return StatusCode(500, "An unexpected error occurred while deleting all file type settings");
            }
        }
    }
}
