using PolyBucket.Api.Features.Filaments.UpdateFilament.Repository;

namespace PolyBucket.Api.Features.Filaments.UpdateFilament.Domain;

public interface IUpdateFilamentService
{
    Task<Filament?> UpdateAsync(UpdateFilamentCommand command, CancellationToken cancellationToken = default);
}

public class UpdateFilamentService(IUpdateFilamentRepository repository) : IUpdateFilamentService
{
    public Task<Filament?> UpdateAsync(UpdateFilamentCommand command, CancellationToken cancellationToken = default) =>
        repository.UpdateAsync(command, cancellationToken);
}
