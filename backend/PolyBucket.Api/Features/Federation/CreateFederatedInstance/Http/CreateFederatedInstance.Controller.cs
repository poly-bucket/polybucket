using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.Federation.CreateFederatedInstance.Domain;
using PolyBucket.Api.Features.Federation.GetFederatedInstance.Http;

namespace PolyBucket.Api.Features.Federation.CreateFederatedInstance.Http;

[ApiController]
[Route("api/federation/instances")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Federation")]
public class CreateFederatedInstanceController(ICreateFederatedInstanceService service) : ControllerBase
{
    /// <summary>
    /// Create a new federated instance connection
    /// </summary>
    [HttpPost]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(typeof(CreateFederatedInstanceResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<CreateFederatedInstanceResponse>> CreateFederatedInstance(
        [FromBody] CreateFederatedInstanceRequest request,
        CancellationToken cancellationToken = default)
    {
        Guid? userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : null;
        var result = await service.CreateAsync(new CreateFederatedInstanceCommand { Request = request, UserId = userId }, cancellationToken);
        return result.Outcome switch
        {
            CreateFederatedInstanceOutcome.ValidationError => BadRequest(result.ErrorMessage),
            CreateFederatedInstanceOutcome.Unauthorized => Unauthorized(result.ErrorMessage),
            CreateFederatedInstanceOutcome.Created => CreatedAtAction(
                nameof(GetFederatedInstanceController.GetFederatedInstance),
                "GetFederatedInstance",
                new { id = result.Response!.Id },
                result.Response),
            _ => StatusCode(500)
        };
    }
}
