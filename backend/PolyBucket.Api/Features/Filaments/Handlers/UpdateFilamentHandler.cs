using MediatR;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Handlers;

public class UpdateFilamentHandler(PolyBucketDbContext context) : IRequestHandler<UpdateFilamentCommand, Filament?>
{
    private readonly PolyBucketDbContext _context = context;

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