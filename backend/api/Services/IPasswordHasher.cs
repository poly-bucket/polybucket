namespace Api.Services;

public interface IPasswordHasher
{
    string HashPassword(string passwordWithSalt);

    bool VerifyPassword(string password, string salt, string hash);
}