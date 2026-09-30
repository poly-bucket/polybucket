using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Models.GetModelVersions.Domain;

namespace PolyBucket.Api.Features.Models.GetModelVersions.Http;

[ApiController]
[Route("api/models")]
public class GetModelVersionsController(IGetModelVersionsService getModelVersionsService) : ControllerBase
{
    /// <summary>
    /// Lists the versions of a model, newest first, with their files.
    /// </summary>
    /// <remarks>
    /// Follows the same visibility rules as the model itself: public, approved models are visible to anyone;
    /// unlisted models to signed-in users; private, pending, or rejected models only to the owner, admins, and moderators.
    /// Models the caller cannot see return 404 so their existence is not revealed.
    /// </remarks>
    /// <param name="id">The model id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The model's versions.</response>
    /// <response code="404">The model does not exist or is not visible to the caller.</response>
    [HttpGet("{id:guid}/versions")]
    [ProducesResponseType(typeof(GetModelVersionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetModelVersionsResponse>> GetModelVersions(Guid id, CancellationToken cancellationToken)
    {
        Guid? viewerId = Guid.TryParse(User.FindUserIdClaim(), out var userId) ? userId : null;
        var response = await getModelVersionsService.GetModelVersionsAsync(id, viewerId, cancellationToken);
        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }
}
