namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISiteBrandingState
{
    Task<string> GetSiteNameAsync(CancellationToken cancellationToken = default);

    void Invalidate();
}