using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Http;
using PolyBucket.Api.Features.Models.UnlikeModel.Domain;

namespace PolyBucket.Api.Features.Models.UnlikeModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
public class UnlikeModelController(IUnlikeModelService service) : ControllerBase
{
    /// <summary>
    /// Removes the caller's like from a model.
    /// </summary>
    [HttpDelete("{id:guid}/like")]
    [ProducesResponseType(typeof(ModelReactionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UnlikeModel(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await service.UnlikeAsync(id, User, cancellationToken);
        return outcome.Change switch
        {
            ModelReactionChange.NotFound => NotFound(),
            ModelReactionChange.Forbidden => Forbid(),
            _ => Ok(ModelReactionResponse.From(outcome))
        };
    }
}
