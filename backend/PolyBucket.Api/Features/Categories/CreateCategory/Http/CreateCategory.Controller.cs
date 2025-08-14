using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.Categories.CreateCategory.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Http
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_CATEGORIES)]
    public class CreateCategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CreateCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new category for models
        /// </summary>
        /// <param name="command">Category creation command</param>
        /// <returns>Created category information</returns>
        /// <response code="201">Category created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User lacks permission to manage categories</response>
        /// <response code="409">Category with this name already exists</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreateCategoryResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<CreateCategoryResponse>> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, result);
        }

        /// <summary>
        /// Get category by ID (for CreatedAtAction)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category information</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CreateCategoryResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CreateCategoryResponse>> GetCategory(Guid id)
        {
            var query = new GetCategoryByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
