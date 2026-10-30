using PolyBucket.Api.Features.Filaments.GetFilamentById.Domain;

namespace PolyBucket.Api.Features.Filaments.GetFilamentById.Repository;

public interface IGetFilamentByIdRepository
{
    Task<Filament?> GetByIdAsync(GetFilamentByIdQuery query, CancellationToken cancellationToken = default);
}

public class GetFilamentByIdRepository(PolyBucketDbContext context) : IGetFilamentByIdRepository
{
    public async Task<Filament?> GetByIdAsync(GetFilamentByIdQuery query, CancellationToken cancellationToken = default) =>
        await context.Filaments.FindAsync([query.Id], cancellationToken);
}
