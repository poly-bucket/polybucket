using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

public class GetSiteAccessService(
    ISitePrivacyState sitePrivacyState,
    ISiteModelUploadPolicy uploadPolicy) : IGetSiteAccessService
{
    private readonly ISitePrivacyState _sitePrivacyState = sitePrivacyState;
    private readonly ISiteModelUploadPolicy _uploadPolicy = uploadPolicy;

    public async Task<GetSiteAccessResponse> GetSiteAccessAsync(CancellationToken cancellationToken)
    {
        var allowPublicBrowsing = await _sitePrivacyState.IsPublicBrowsingAllowedAsync(cancellationToken);
        var allowAnonymousModelUpload = await _uploadPolicy.IsAnonymousModelUploadAllowedAsync(cancellationToken);
        var requireLoginForUpload = await _uploadPolicy.IsLoginRequiredForUnauthenticatedUploadAsync(cancellationToken);
        var defaultPrivacy = await _uploadPolicy.GetDefaultModelPrivacyAsync(cancellationToken);

        return new GetSiteAccessResponse
        {
            AllowPublicBrowsing = allowPublicBrowsing,
            RequireLoginForUpload = requireLoginForUpload,
            AllowAnonymousModelUpload = allowAnonymousModelUpload,
            DefaultModelPrivacy = defaultPrivacy.ToString()
        };
    }
}