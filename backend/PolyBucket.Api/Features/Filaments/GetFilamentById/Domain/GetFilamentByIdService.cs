namespace PolyBucket.Api.Features.Filaments.GetFilamentById.Domain;

using PolyBucket.Api.Features.Filaments.GetFilamentById.Repository;

public class GetFilamentByIdQuery
{
    public Guid Id { get; set; }
}

public interface IGetFilamentByIdService
{
    Task<Filament?> GetByIdAsync(GetFilamentByIdQuery query, CancellationToken cancellationToken = default);
}

public class GetFilamentByIdService(IGetFilamentByIdRepository repository) : IGetFilamentByIdService
{
    public Task<Filament?> GetByIdAsync(GetFilamentByIdQuery query, CancellationToken cancellationToken = default) =>
        repository.GetByIdAsync(query, cancellationToken);
}
