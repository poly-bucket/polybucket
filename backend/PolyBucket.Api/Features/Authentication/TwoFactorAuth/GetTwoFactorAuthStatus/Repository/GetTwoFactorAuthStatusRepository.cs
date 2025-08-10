using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Repository
{
    public interface IGetTwoFactorAuthStatusRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
    }

    public class GetTwoFactorAuthStatusRepository : IGetTwoFactorAuthStatusRepository
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<GetTwoFactorAuthStatusRepository> _logger;

        public GetTwoFactorAuthStatusRepository(PolyBucketDbContext context, ILogger<GetTwoFactorAuthStatusRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId)
        {
            return await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .Include(tfa => tfa.User)
                .FirstOrDefaultAsync(tfa => tfa.UserId == userId);
        }
    }
} 