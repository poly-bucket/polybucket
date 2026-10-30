namespace PolyBucket.Api.Features.Plugins.Services;

public interface IMarketplaceBaseUrlResolver
{
    Task<string> ResolveAsync(CancellationToken cancellationToken = default);
}
