using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.Queries;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class GetFilamentByIdController : ControllerBase
{
    private readonly IMediator _mediator;

    public GetFilamentByIdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var filament = await _mediator.Send(new GetFilamentByIdQuery { Id = id });
        return filament != null ? Ok(filament) : NotFound();
    }
} 