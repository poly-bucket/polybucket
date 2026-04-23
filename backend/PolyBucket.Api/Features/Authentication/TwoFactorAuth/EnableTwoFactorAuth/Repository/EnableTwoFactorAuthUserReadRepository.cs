using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository;

public class EnableTwoFactorAuthUserReadRepository(PolyBucketDbContext context) : IEnableTwoFactorAuthUserReadRepository
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
