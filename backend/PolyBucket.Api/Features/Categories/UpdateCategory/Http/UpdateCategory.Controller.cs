using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.Categories.UpdateCategory.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.UpdateCategory.Http
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_CATEGORIES)]
    public class UpdateCategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UpdateCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Update a category by ID
        /// </summary>
        /// <param name="id">Category ID to update</param>
        /// <param name="command">Category update command</param>
        /// <returns>Updated category information</returns>
        /// <response code="200">Category updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User lacks permission to manage categories</response>
        /// <response code="404">Category not found</response>
        /// <response code="409">Category with this name already exists</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateCategoryResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<UpdateCategoryResponse>> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
