using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository;

public class InitializeTwoFactorAuthUserReadRepository(PolyBucketDbContext context) : IInitializeTwoFactorAuthUserReadRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users
            .Include(u => u.Settings)
            .Include(u => u.Logins)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}
