namespace PolyBucket.Api.Features.Filaments.GetAllFilaments.Repository;

using Microsoft.EntityFrameworkCore;

public interface IGetAllFilamentsRepository
{
    Task<List<Filament>> GetAllAsync(CancellationToken cancellationToken = default);
}

public class GetAllFilamentsRepository(PolyBucketDbContext context) : IGetAllFilamentsRepository
{
    public Task<List<Filament>> GetAllAsync(CancellationToken cancellationToken = default) =>
        context.Filaments.ToListAsync();
}
