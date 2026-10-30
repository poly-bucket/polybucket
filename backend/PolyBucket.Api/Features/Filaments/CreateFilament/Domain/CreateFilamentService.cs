using PolyBucket.Api.Features.Filaments.CreateFilament.Domain;
using PolyBucket.Api.Features.Filaments.CreateFilament.Repository;

namespace PolyBucket.Api.Features.Filaments.CreateFilament.Domain;

public interface ICreateFilamentService
{
    Task<Filament> CreateAsync(CreateFilamentCommand command, CancellationToken cancellationToken = default);
}

public class CreateFilamentService(ICreateFilamentRepository repository) : ICreateFilamentService
{
    public Task<Filament> CreateAsync(CreateFilamentCommand command, CancellationToken cancellationToken = default)
    {
        var filament = new Filament
        {
            Manufacturer = command.Manufacturer,
            Type = command.Type,
            Color = command.Color,
            Diameter = command.Diameter
        };
        return repository.CreateAsync(filament, cancellationToken);
    }
}
