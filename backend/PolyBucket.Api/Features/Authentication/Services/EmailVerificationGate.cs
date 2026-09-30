using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface IEmailVerificationGate
    {
        Task<bool> IsSatisfiedAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    }

    public class EmailVerificationGate(IEmailSettingsResolver emailSettingsResolver, PolyBucketDbContext context) : IEmailVerificationGate
    {
        private readonly IEmailSettingsResolver _emailSettingsResolver = emailSettingsResolver;
        private readonly PolyBucketDbContext _context = context;

        public async Task<bool> IsSatisfiedAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return true;
            }

            var settings = await _emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);
            if (!settings.CanDeliver || !settings.RequireEmailVerification)
            {
                return true;
            }

            if (string.Equals(principal.FindFirst(TokenService.EmailVerifiedClaim)?.Value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!Guid.TryParse(principal.FindUserIdClaim(), out var userId))
            {
                return false;
            }

            var verifiedAt = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.EmailVerifiedAt)
                .FirstOrDefaultAsync(cancellationToken);
            return verifiedAt.HasValue;
        }
    }
}
