using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class GetAllFilaments : ControllerBase
{
    private readonly IMediator _mediator;

    public GetAllFilaments(IMediator mediator)
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

public class GetAllFilamentsQuery : IRequest<List<Filament>>
{
}

public class GetAllFilamentsHandler : IRequestHandler<GetAllFilamentsQuery, List<Filament>>
{
    private readonly PolyBucketDbContext _context;

    public GetAllFilamentsHandler(PolyBucketDbContext context)
    {
        _context = context;
    }

    public async Task<List<Filament>> Handle(GetAllFilamentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Filaments.ToListAsync(cancellationToken);
    }
} 