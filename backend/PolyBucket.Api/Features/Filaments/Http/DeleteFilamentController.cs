using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.Commands;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class DeleteFilamentController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeleteFilamentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteFilamentCommand { Id = id });
        return success ? NoContent() : NotFound();
    }
} 