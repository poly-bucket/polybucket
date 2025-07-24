using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class CreateFilamentController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFilamentCommand command)
    {
        var filament = await _mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = filament.Id }, filament);
    }
} 