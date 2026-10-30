using PolyBucket.Api.Features.Filaments.DeleteFilament.Domain;

namespace PolyBucket.Api.Features.Filaments.DeleteFilament.Repository;

public interface IDeleteFilamentRepository
{
    Task<bool> DeleteAsync(DeleteFilamentCommand command, CancellationToken cancellationToken = default);
}

public class DeleteFilamentRepository(PolyBucketDbContext context) : IDeleteFilamentRepository
{
    public async Task<bool> DeleteAsync(DeleteFilamentCommand command, CancellationToken cancellationToken = default)
    {
        var filament = await context.Filaments.FindAsync(command.Id);
        if (filament == null)
        {
            return false;
        }

        context.Filaments.Remove(filament);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
