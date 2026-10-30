using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Register.Repository;

public interface IRegisterRepository
{
    Task<bool> IsEmailTakenAsync(string email);
    Task<bool> IsUsernameTakenAsync(string username);
    Task<Guid?> GetDefaultUserRoleIdAsync(CancellationToken cancellationToken);
    Task<User> CreateUserAsync(User user);
    Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken token);
    Task<RefreshTokenModel> CreateRefreshTokenAsync(RefreshTokenModel refreshToken);
    Task CreateLoginRecordAsync(UserLogin userLogin);
}
