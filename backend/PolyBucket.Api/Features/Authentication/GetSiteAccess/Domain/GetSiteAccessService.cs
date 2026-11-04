using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

public class GetSiteAccessService(ISitePrivacyState sitePrivacyState) : IGetSiteAccessService
{
    private readonly ISitePrivacyState _sitePrivacyState = sitePrivacyState;

    public async Task<GetSiteAccessResponse> GetSiteAccessAsync(CancellationToken cancellationToken)
    {
        var allowed = await _sitePrivacyState.IsPublicBrowsingAllowedAsync(cancellationToken);
        return new GetSiteAccessResponse { AllowPublicBrowsing = allowed };
    }
}
