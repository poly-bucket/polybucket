using PolyBucket.Api.Features.Filaments.UpdateFilament.Domain;

namespace PolyBucket.Api.Features.Filaments.UpdateFilament.Repository;

public interface IUpdateFilamentRepository
{
    Task<Filament?> UpdateAsync(UpdateFilamentCommand command, CancellationToken cancellationToken = default);
}

public class UpdateFilamentRepository(PolyBucketDbContext context) : IUpdateFilamentRepository
{
    public async Task<Filament?> UpdateAsync(UpdateFilamentCommand command, CancellationToken cancellationToken = default)
    {
        var filament = await context.Filaments.FindAsync(command.Id);
        if (filament == null)
        {
            return null;
        }

        filament.Manufacturer = command.Manufacturer;
        filament.Type = command.Type;
        filament.Color = command.Color;
        filament.Diameter = command.Diameter;
        await context.SaveChangesAsync(cancellationToken);
        return filament;
    }
}
