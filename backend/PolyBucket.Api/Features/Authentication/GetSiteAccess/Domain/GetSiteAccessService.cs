using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

public class GetSiteAccessService(
    ISitePrivacyState sitePrivacyState,
    ISiteModelUploadPolicy uploadPolicy,
    ISiteModelDownloadPolicy downloadPolicy,
    IModelMetadataPolicy metadataPolicy) : IGetSiteAccessService
{
    private readonly ISitePrivacyState _sitePrivacyState = sitePrivacyState;
    private readonly ISiteModelUploadPolicy _uploadPolicy = uploadPolicy;
    private readonly ISiteModelDownloadPolicy _downloadPolicy = downloadPolicy;
    private readonly IModelMetadataPolicy _metadataPolicy = metadataPolicy;

    public async Task<GetSiteAccessResponse> GetSiteAccessAsync(CancellationToken cancellationToken)
    {
        var allowPublicBrowsing = await _sitePrivacyState.IsPublicBrowsingAllowedAsync(cancellationToken);
        var allowAnonymousModelUpload = await _uploadPolicy.IsAnonymousModelUploadAllowedAsync(cancellationToken);
        var requireLoginForUpload = await _uploadPolicy.IsLoginRequiredForUnauthenticatedUploadAsync(cancellationToken);
        var allowAnonymousModelDownload = await _downloadPolicy.IsAnonymousModelDownloadAllowedAsync(cancellationToken);
        var modelDownloadsEnabled = await _downloadPolicy.AreModelDownloadsEnabledAsync(cancellationToken);
        var defaultPrivacy = await _uploadPolicy.GetDefaultModelPrivacyAsync(cancellationToken);
        var metadata = await _metadataPolicy.GetSnapshotAsync(cancellationToken);

        return new GetSiteAccessResponse
        {
            AllowPublicBrowsing = allowPublicBrowsing,
            RequireLoginForUpload = requireLoginForUpload,
            AllowAnonymousModelUpload = allowAnonymousModelUpload,
            AllowAnonymousModelDownload = allowAnonymousModelDownload,
            ModelDownloadsEnabled = modelDownloadsEnabled,
            DefaultModelPrivacy = defaultPrivacy.ToString(),
            ModelMetadataRules = new ModelMetadataRulesDto
            {
                RequireModelDescription = metadata.RequireModelDescription,
                MinDescriptionLength = metadata.MinDescriptionLength,
                MaxDescriptionLength = metadata.MaxDescriptionLength,
                RequireLicenseSelection = metadata.RequireLicenseSelection,
                AllowCustomLicenses = metadata.AllowCustomLicenses,
                AllowNsfwContent = metadata.AllowNsfwContent,
                AllowAiGeneratedContent = metadata.AllowAiGeneratedContent,
                RequireThumbnail = metadata.RequireThumbnail,
                MaxTagsPerModel = metadata.MaxTagsPerModel,
                RequireModelTags = metadata.RequireModelTags,
                MaxCategoriesPerModel = metadata.MaxCategoriesPerModel,
                RequireCategorySelection = metadata.RequireCategorySelection
            }
        };
    }
}