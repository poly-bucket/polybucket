using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.Queries;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class GetAllFilamentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GetAllFilamentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var filaments = await _mediator.Send(new GetAllFilamentsQuery());
        return Ok(filaments);
    }
} 