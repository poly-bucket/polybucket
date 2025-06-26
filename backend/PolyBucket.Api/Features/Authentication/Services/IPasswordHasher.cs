namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface IPasswordHasher
    {
        string GenerateSalt();
        string HashPassword(string password, string salt);
        bool VerifyPassword(string password, string hash);
    }
} 