using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Authentication.Register.Domain;
using PolyBucket.Database.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Services;

public class UserRegistrationGate(PolyBucketDbContext context) : IUserRegistrationGate
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<bool> IsRegistrationAllowedAsync(CancellationToken cancellationToken = default)
    {
        var setup = await _context.SystemSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        return setup is { AllowUserRegistration: true };
    }

    public async Task EnsureRegistrationAllowedAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsRegistrationAllowedAsync(cancellationToken))
        {
            throw new UserRegistrationClosedException();
        }
    }
}
