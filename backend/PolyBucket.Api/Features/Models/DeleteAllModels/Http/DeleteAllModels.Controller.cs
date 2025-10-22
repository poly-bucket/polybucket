using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.DeleteAllModels.Domain;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace PolyBucket.Api.Features.Models.DeleteAllModels.Http
{
    [Authorize]
    [ApiController]
    [Route("api/admin/models")]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_USERS)]
    public class DeleteAllModelsController(IDeleteAllModelsService deleteAllModelsService, ILogger<DeleteAllModelsController> logger) : ControllerBase
    {
        private readonly IDeleteAllModelsService _deleteAllModelsService = deleteAllModelsService;
        private readonly ILogger<DeleteAllModelsController> _logger = logger;

        /// <summary>
        /// Delete ALL models from the system (DANGEROUS OPERATION - Admin only)
        /// </summary>
        /// <param name="request">Delete request with admin password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the deletion operation</returns>
        [HttpDelete("delete-all")]
        [ProducesResponseType(typeof(DeleteAllModelsResponse), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeleteAllModelsResponse>> DeleteAllModels(
            [FromBody] DeleteAllModelsRequest request, 
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogWarning("Admin {AdminId} is attempting to delete ALL models", 
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var result = await _deleteAllModelsService.DeleteAllModelsAsync(request, User, cancellationToken);
                
                if (result.Success)
                {
                    _logger.LogCritical("CRITICAL: All models have been deleted by admin {AdminId}. Deleted count: {DeletedCount}", 
                        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, result.DeletedCount);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to delete all models: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while deleting all models");
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting all models");
                return StatusCode(500, "An unexpected error occurred while deleting all models");
            }
        }
    }
}
