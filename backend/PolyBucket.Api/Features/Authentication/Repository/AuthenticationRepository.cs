using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Authentication.Repository
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly PolyBucketDbContext _context;

        public AuthenticationRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task CreateLoginRecordAsync(UserLogin userLogin)
        {
            await _context.UserLogins.AddAsync(userLogin);
            await _context.SaveChangesAsync();
        }
    }
} 