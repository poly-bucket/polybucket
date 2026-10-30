using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.Categories.CreateCategory.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

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
            return CreatedAtRoute(
                routeName: "GetCategoryById",
                routeValues: new { id = result.Id },
                value: result);
        }
    }
}
