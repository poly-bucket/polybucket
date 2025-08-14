using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.Categories.DeleteCategory.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteCategory.Http
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_CATEGORIES)]
    public class DeleteCategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DeleteCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Delete a category by ID
        /// </summary>
        /// <param name="id">Category ID to delete</param>
        /// <returns>Deletion result</returns>
        /// <response code="200">Category deleted successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User lacks permission to manage categories</response>
        /// <response code="404">Category not found</response>
        /// <response code="409">Category cannot be deleted (in use by models)</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(DeleteCategoryResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<DeleteCategoryResponse>> DeleteCategory(Guid id)
        {
            var command = new DeleteCategoryCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
