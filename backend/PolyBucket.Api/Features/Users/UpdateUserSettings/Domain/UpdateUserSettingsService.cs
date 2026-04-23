using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Repository;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;

public class UpdateUserSettingsService(IUpdateUserSettingsRepository repository) : IUpdateUserSettingsService
{
    public Task UpdateAsync(UpdateUserSettingsCommand command, CancellationToken cancellationToken = default)
    {
        return repository.ApplyUpdateAsync(command, cancellationToken);
    }
}
