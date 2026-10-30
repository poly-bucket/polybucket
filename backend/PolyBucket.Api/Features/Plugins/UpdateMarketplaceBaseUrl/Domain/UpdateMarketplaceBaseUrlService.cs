using PolyBucket.Api.Features.Plugins.UpdateMarketplaceBaseUrl.Repository;

namespace PolyBucket.Api.Features.Plugins.UpdateMarketplaceBaseUrl.Domain;

public class UpdateMarketplaceBaseUrlService(IUpdateMarketplaceBaseUrlRepository repository) : IUpdateMarketplaceBaseUrlService
{
    public UpdateMarketplaceBaseUrlResult ValidateAndNormalize(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return new UpdateMarketplaceBaseUrlResult { IsValid = false, ErrorMessage = "Base URL must be a valid absolute URL" };
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return new UpdateMarketplaceBaseUrlResult { IsValid = false, ErrorMessage = "Base URL must use http or https" };
        }

        var normalized = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        return new UpdateMarketplaceBaseUrlResult { IsValid = true, NormalizedBaseUrl = normalized };
    }

    public Task SaveAsync(string normalizedBaseUrl, CancellationToken cancellationToken = default) =>
        repository.SaveBaseUrlAsync(normalizedBaseUrl, cancellationToken);
}
