using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Categories.DeleteAllCategories.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteAllCategories.Http
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_CATEGORIES)]
    public class DeleteAllCategoriesController(
        IDeleteAllCategoriesService deleteAllCategoriesService,
        ILogger<DeleteAllCategoriesController> logger) : ControllerBase
    {
        private readonly IDeleteAllCategoriesService _deleteAllCategoriesService = deleteAllCategoriesService;
        private readonly ILogger<DeleteAllCategoriesController> _logger = logger;

        /// <summary>
        /// Delete ALL categories from the system (DANGEROUS OPERATION - Admin only)
        /// </summary>
        /// <param name="request">Delete request with admin password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the deletion operation</returns>
        [HttpDelete("delete-all")]
        [ProducesResponseType(typeof(DeleteAllCategoriesResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DeleteAllCategoriesResponse>> DeleteAllCategories(
            [FromBody] DeleteAllCategoriesRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogWarning(
                    "Admin {AdminId} is attempting to delete ALL categories",
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var result = await _deleteAllCategoriesService.DeleteAllCategoriesAsync(
                    request,
                    User,
                    cancellationToken);

                if (result.Success)
                {
                    _logger.LogCritical(
                        "CRITICAL: All categories deleted by admin {AdminId}. Deleted count: {DeletedCount}",
                        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                        result.DeletedCount);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to delete all categories: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting all categories");
                return StatusCode(500, "An unexpected error occurred while deleting all categories");
            }
        }
    }
}
