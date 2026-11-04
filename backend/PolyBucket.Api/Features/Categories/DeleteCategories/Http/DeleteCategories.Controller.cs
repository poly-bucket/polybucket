using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.Categories.DeleteCategories.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteCategories.Http
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_CATEGORIES)]
    public class DeleteCategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DeleteCategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Delete multiple categories by ID
        /// </summary>
        /// <param name="command">Category IDs to delete</param>
        /// <returns>Per-category deletion results</returns>
        /// <response code="200">Bulk delete completed (individual items may have failed)</response>
        /// <response code="400">No category IDs were provided</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User lacks permission to manage categories</response>
        [HttpPost("bulk-delete")]
        [ProducesResponseType(typeof(DeleteCategoriesResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<DeleteCategoriesResponse>> DeleteCategories(
            [FromBody] DeleteCategoriesCommand command)
        {
            if (command.Ids == null || !command.Ids.Any())
            {
                return BadRequest("At least one category id is required");
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
