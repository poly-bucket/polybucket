using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.ForgotPassword.Repository;

public interface IForgotPasswordRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<PasswordResetToken> CreatePasswordResetTokenAsync(PasswordResetToken token);
}
