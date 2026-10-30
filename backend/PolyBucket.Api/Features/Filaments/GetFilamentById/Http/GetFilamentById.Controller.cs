using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.GetFilamentById.Domain;

namespace PolyBucket.Api.Features.Filaments.GetFilamentById.Http;

[Route("api/filaments")]
[ApiController]
public class GetFilamentByIdController(IGetFilamentByIdService service) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var filament = await service.GetByIdAsync(new GetFilamentByIdQuery { Id = id }, cancellationToken);
        return filament == null ? NotFound() : Ok(filament);
    }
}
