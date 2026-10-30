namespace PolyBucket.Api.Features.Plugins.GetOAuthAuthorizationUrl.Repository;

public interface IGetOAuthAuthorizationUrlRepository
{
    bool IsProviderRegistered(string providerName);
}
