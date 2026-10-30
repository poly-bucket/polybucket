using PolyBucket.Api.Features.Filaments.DeleteFilament.Repository;

namespace PolyBucket.Api.Features.Filaments.DeleteFilament.Domain;

public class DeleteFilamentCommand
{
    public Guid Id { get; set; }
}

public interface IDeleteFilamentService
{
    Task<bool> DeleteAsync(DeleteFilamentCommand command, CancellationToken cancellationToken = default);
}

public class DeleteFilamentService(IDeleteFilamentRepository repository) : IDeleteFilamentService
{
    public Task<bool> DeleteAsync(DeleteFilamentCommand command, CancellationToken cancellationToken = default) =>
        repository.DeleteAsync(command, cancellationToken);
}
