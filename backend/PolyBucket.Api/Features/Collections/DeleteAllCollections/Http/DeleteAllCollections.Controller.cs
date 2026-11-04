using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Collections.DeleteAllCollections.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.DeleteAllCollections.Http
{
    [ApiController]
    [Route("api/admin/collections")]
    [Authorize]
    [RequirePermission(PermissionConstants.COLLECTION_DELETE_ANY)]
    public class DeleteAllCollectionsController(
        IDeleteAllCollectionsService deleteAllCollectionsService,
        ILogger<DeleteAllCollectionsController> logger) : ControllerBase
    {
        private readonly IDeleteAllCollectionsService _deleteAllCollectionsService = deleteAllCollectionsService;
        private readonly ILogger<DeleteAllCollectionsController> _logger = logger;

        /// <summary>
        /// Delete ALL collections from the system for all users (DANGEROUS OPERATION - Admin only)
        /// </summary>
        /// <param name="request">Delete request with admin password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the deletion operation</returns>
        [HttpDelete("delete-all")]
        [ProducesResponseType(typeof(DeleteAllCollectionsResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DeleteAllCollectionsResponse>> DeleteAllCollections(
            [FromBody] DeleteAllCollectionsRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogWarning(
                    "Admin {AdminId} is attempting to delete ALL collections",
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var result = await _deleteAllCollectionsService.DeleteAllCollectionsAsync(
                    request,
                    User,
                    cancellationToken);

                if (result.Success)
                {
                    _logger.LogCritical(
                        "CRITICAL: All collections deleted by admin {AdminId}. Deleted count: {DeletedCount}",
                        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                        result.DeletedCount);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to delete all collections: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting all collections");
                return StatusCode(500, "An unexpected error occurred while deleting all collections");
            }
        }
    }
}
