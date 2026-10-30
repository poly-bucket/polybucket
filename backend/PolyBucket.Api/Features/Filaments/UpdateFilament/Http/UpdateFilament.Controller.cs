using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.UpdateFilament.Domain;

namespace PolyBucket.Api.Features.Filaments.UpdateFilament.Http;

[Route("api/filaments")]
[ApiController]
public class UpdateFilamentController(IUpdateFilamentService service) : ControllerBase
{
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFilamentCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;
        var filament = await service.UpdateAsync(command, cancellationToken);
        return filament == null ? NotFound() : Ok(filament);
    }
}
