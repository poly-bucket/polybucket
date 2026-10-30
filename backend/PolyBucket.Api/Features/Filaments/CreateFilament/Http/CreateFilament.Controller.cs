using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.CreateFilament.Domain;

namespace PolyBucket.Api.Features.Filaments.CreateFilament.Http;

[Route("api/filaments")]
[ApiController]
public class CreateFilamentController(ICreateFilamentService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFilamentCommand command, CancellationToken cancellationToken = default)
    {
        var filament = await service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = filament.Id }, filament);
    }
}
