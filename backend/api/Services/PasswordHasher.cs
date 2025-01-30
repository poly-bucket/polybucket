namespace Api.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string passwordWithSalt)
    {
        return BCrypt.Net.BCrypt.HashPassword(passwordWithSalt);
    }

    public bool VerifyPassword(string password, string salt, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password + salt, hash);
    }
}