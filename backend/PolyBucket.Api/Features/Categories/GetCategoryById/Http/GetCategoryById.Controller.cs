using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Categories.GetCategoryById.Domain;
using PolyBucket.Api.Features.Categories.GetCategoryById.Http;

namespace PolyBucket.Api.Features.Categories.GetCategoryById.Http;

[ApiController]
[Route("api/admin/categories")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_CATEGORIES)]
public class GetCategoryByIdController(IGetCategoryByIdService service) : ControllerBase
{
    /// <summary>
    /// Get category by ID.
    /// </summary>
    [HttpGet("{id}", Name = "GetCategoryById")]
    [ProducesResponseType(typeof(GetCategoryByIdResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetCategoryByIdResponseDto>> GetCategoryById(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await service.GetCategoryByIdAsync(id, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }
}
