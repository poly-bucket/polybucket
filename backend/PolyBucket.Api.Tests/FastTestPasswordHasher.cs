using PolyBucket.Api.Features.Authentication.Services;

namespace PolyBucket.Api.Tests;

internal sealed class FastTestPasswordHasher : IPasswordHasher
{
    public string GenerateSalt() => BCrypt.Net.BCrypt.GenerateSalt(TestPasswordHashing.BcryptWorkFactor);

    public string HashPassword(string password, string salt) => BCrypt.Net.BCrypt.HashPassword(password, salt);

    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
