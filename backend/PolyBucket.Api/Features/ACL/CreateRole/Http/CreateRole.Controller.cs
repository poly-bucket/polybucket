using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.CreateRole.Domain;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.GetRole.Http;

namespace PolyBucket.Api.Features.ACL.CreateRole.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class CreateRoleController(ICreateRoleService service) : ControllerBase
{
    /// <summary>
    /// Create a new role.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var (outcome, role, error) = await service.CreateRoleAsync(userId, request, cancellationToken);
        return outcome switch
        {
            CreateRoleOutcome.Unauthorized => Unauthorized(),
            CreateRoleOutcome.Invalid => BadRequest(error),
            _ => CreatedAtAction(nameof(GetRoleController.GetRole), "GetRole", new { id = role!.Id }, role)
        };
    }
}
