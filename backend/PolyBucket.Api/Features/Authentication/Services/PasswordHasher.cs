using BCrypt.Net;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password, out string salt)
        {
            salt = Salt.Generate();
            return BCrypt.HashPassword(password + salt);
        }

        public bool VerifyPassword(string password, string salt, string hash)
        {
            return BCrypt.Verify(password + salt, hash);
        }
    }
} 