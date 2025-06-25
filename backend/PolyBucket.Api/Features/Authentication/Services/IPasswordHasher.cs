namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password, out string salt);
        bool VerifyPassword(string password, string salt, string hash);
    }
} 