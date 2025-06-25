using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class GetFilamentById : ControllerBase
{
    private readonly IMediator _mediator;

    public GetFilamentById(IMediator mediator)
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

public class GetFilamentByIdQuery : IRequest<Filament?>
{
    public Guid Id { get; set; }
}

public class GetFilamentByIdHandler : IRequestHandler<GetFilamentByIdQuery, Filament?>
{
    private readonly PolyBucketDbContext _context;

    public GetFilamentByIdHandler(PolyBucketDbContext context)
    {
        _context = context;
    }

    public async Task<Filament?> Handle(GetFilamentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Filaments.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
    }
} 