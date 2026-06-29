namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISitePrivacyState
{
    Task<bool> IsPublicBrowsingAllowedAsync(CancellationToken cancellationToken = default);

    void Invalidate();
}
