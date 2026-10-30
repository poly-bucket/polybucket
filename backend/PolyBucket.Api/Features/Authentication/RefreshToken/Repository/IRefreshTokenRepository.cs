using PolyBucket.Database.Entities.Models;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.RefreshToken.Repository;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenModel?> GetRefreshTokenAsync(string token);
    Task<User?> GetUserByEmailAsync(string email);
    Task RevokeRefreshTokenAsync(string token, string reason, string revokedByIp);
    Task<RefreshTokenModel> CreateRefreshTokenAsync(RefreshTokenModel refreshToken);
}
