using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.GetAllFilaments.Domain;

namespace PolyBucket.Api.Features.Filaments.GetAllFilaments.Http;

[Route("api/filaments")]
[ApiController]
public class GetAllFilamentsController(IGetAllFilamentsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default) =>
        Ok(await service.GetAllAsync(cancellationToken));
}
