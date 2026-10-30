namespace PolyBucket.Api.Features.Filaments.GetAllFilaments.Domain;

using PolyBucket.Api.Features.Filaments.GetAllFilaments.Repository;

public class GetAllFilamentsQuery;

public interface IGetAllFilamentsService
{
    Task<List<Filament>> GetAllAsync(CancellationToken cancellationToken = default);
}

public class GetAllFilamentsService(IGetAllFilamentsRepository repository) : IGetAllFilamentsService
{
    public Task<List<Filament>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);
}
