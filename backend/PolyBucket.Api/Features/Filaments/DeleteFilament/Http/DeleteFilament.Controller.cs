using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.DeleteFilament.Domain;

namespace PolyBucket.Api.Features.Filaments.DeleteFilament.Http;

[Route("api/filaments")]
[ApiController]
public class DeleteFilamentController(IDeleteFilamentService service) : ControllerBase
{
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await service.DeleteAsync(new DeleteFilamentCommand { Id = id }, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
