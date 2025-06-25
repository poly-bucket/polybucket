using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Printers.Domain.Enums;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class UpdateFilament : ControllerBase
{
    private readonly IMediator _mediator;

    public UpdateFilament(IMediator mediator)
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

public class UpdateFilamentCommand : IRequest<Filament?>
{
    public Guid Id { get; set; }
    public required string Manufacturer { get; set; }
    public MaterialType Type { get; set; }
    public required string Color { get; set; }
    public required string Diameter { get; set; }
}

public class UpdateFilamentHandler : IRequestHandler<UpdateFilamentCommand, Filament?>
{
    private readonly PolyBucketDbContext _context;

    public UpdateFilamentHandler(PolyBucketDbContext context)
    {
        _context = context;
    }

    public async Task<Filament?> Handle(UpdateFilamentCommand request, CancellationToken cancellationToken)
    {
        var filament = await _context.Filaments.FindAsync(request.Id);

        if (filament == null)
        {
            return null;
        }

        filament.Manufacturer = request.Manufacturer;
        filament.Type = request.Type;
        filament.Color = request.Color;
        filament.Diameter = request.Diameter;

        await _context.SaveChangesAsync(cancellationToken);

        return filament;
    }
} 