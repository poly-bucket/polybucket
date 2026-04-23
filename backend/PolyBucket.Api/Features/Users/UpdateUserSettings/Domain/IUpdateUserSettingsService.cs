using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;

public interface IUpdateUserSettingsService
{
    Task UpdateAsync(UpdateUserSettingsCommand command, CancellationToken cancellationToken = default);
}
