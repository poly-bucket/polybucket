using MediatR;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Handlers;

public class CreateFilamentHandler(PolyBucketDbContext context) : IRequestHandler<CreateFilamentCommand, Filament>
{
    private readonly PolyBucketDbContext _context = context;

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