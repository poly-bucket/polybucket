using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.GetUserSettings.Repository;

public class GetUserSettingsRepository(PolyBucketDbContext context) : IGetUserSettingsRepository
{
    public Task<UserSettings?> GetSettingsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }
}
