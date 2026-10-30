namespace PolyBucket.Api.Features.Plugins.UpdateMarketplaceBaseUrl.Domain;

public sealed class UpdateMarketplaceBaseUrlResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public string NormalizedBaseUrl { get; init; } = string.Empty;
}

public interface IUpdateMarketplaceBaseUrlService
{
    UpdateMarketplaceBaseUrlResult ValidateAndNormalize(string baseUrl);

    Task SaveAsync(string normalizedBaseUrl, CancellationToken cancellationToken = default);
}
