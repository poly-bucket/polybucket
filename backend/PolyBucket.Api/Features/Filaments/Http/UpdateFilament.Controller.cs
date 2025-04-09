using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class UpdateFilamentController : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateFilamentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFilamentCommand command)
    {
        command.Id = id;
        var filament = await _mediator.Send(command);
        return filament != null ? Ok(filament) : NotFound();
    }
} 