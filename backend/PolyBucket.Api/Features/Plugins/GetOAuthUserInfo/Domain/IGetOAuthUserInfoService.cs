using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;

public enum OAuthRequestStatus { Ok, BadRequest, NotFound }
public sealed class OAuthSimpleOutcome<T> { public OAuthRequestStatus Status { get; init; } public T? Value { get; init; } public object? ErrorBody { get; init; } }

public interface IGetOAuthUserInfoService
{
    Task<OAuthSimpleOutcome<OAuthUserInfo>> GetUserInfoAsync(string providerName, string accessToken);
}
