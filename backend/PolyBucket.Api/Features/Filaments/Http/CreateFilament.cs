using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Printers.Domain.Enums;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class CreateFilament : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateFilament(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFilamentCommand command)
    {
        var filament = await _mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = filament.Id }, filament);
    }
}

public class CreateFilamentCommand : IRequest<Filament>
{
    public required string Manufacturer { get; set; }
    public MaterialType Type { get; set; }
    public required string Color { get; set; }
    public required string Diameter { get; set; }
}

public class CreateFilamentHandler : IRequestHandler<CreateFilamentCommand, Filament>
{
    private readonly PolyBucketDbContext _context;

    public CreateFilamentHandler(PolyBucketDbContext context)
    {
        _context = context;
    }

    public async Task<Filament> Handle(CreateFilamentCommand request, CancellationToken cancellationToken)
    {
        var filament = new Filament
        {
            Manufacturer = request.Manufacturer,
            Type = request.Type,
            Color = request.Color,
            Diameter = request.Diameter
        };

        _context.Filaments.Add(filament);
        await _context.SaveChangesAsync(cancellationToken);

        return filament;
    }
} 