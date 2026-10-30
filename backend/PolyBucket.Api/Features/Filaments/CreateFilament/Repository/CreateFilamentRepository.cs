namespace PolyBucket.Api.Features.Filaments.CreateFilament.Repository;

public interface ICreateFilamentRepository
{
    Task<Filament> CreateAsync(Filament filament, CancellationToken cancellationToken = default);
}

public class CreateFilamentRepository(PolyBucketDbContext context) : ICreateFilamentRepository
{
    public async Task<Filament> CreateAsync(Filament filament, CancellationToken cancellationToken = default)
    {
        context.Filaments.Add(filament);
        await context.SaveChangesAsync(cancellationToken);
        return filament;
    }
}
