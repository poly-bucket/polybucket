using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.Categories.GetCategories.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.GetCategories.Http
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_CATEGORIES)]
    public class GetCategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GetCategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get categories with pagination and search
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 20)</param>
        /// <param name="searchTerm">Search term to filter categories by name</param>
        /// <returns>Paginated list of categories</returns>
        /// <response code="200">Categories retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User lacks permission to manage categories</response>
        [HttpGet]
        [ProducesResponseType(typeof(GetCategoriesResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<GetCategoriesResponse>> GetCategories(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetCategoriesQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
