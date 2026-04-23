using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Repository;

public interface IUpdateUserSettingsRepository
{
    Task ApplyUpdateAsync(UpdateUserSettingsCommand command, CancellationToken cancellationToken = default);
}
