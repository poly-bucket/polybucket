using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.GetUserSettings.Repository;

public interface IGetUserSettingsRepository
{
    Task<UserSettings?> GetSettingsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
