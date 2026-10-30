using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Http;
using PolyBucket.Api.Features.Models.RemoveModelDislike.Domain;

namespace PolyBucket.Api.Features.Models.RemoveModelDislike.Http;

[Authorize]
[ApiController]
[Route("api/models")]
public class RemoveModelDislikeController(IRemoveModelDislikeService service) : ControllerBase
{
    [HttpDelete("{id:guid}/dislike")]
    [ProducesResponseType(typeof(ModelReactionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveDislike(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await service.RemoveDislikeAsync(id, User, cancellationToken);
        return outcome.Change switch
        {
            ModelReactionChange.NotFound => NotFound(),
            ModelReactionChange.Forbidden => Forbid(),
            _ => Ok(ModelReactionResponse.From(outcome))
        };
    }
}
