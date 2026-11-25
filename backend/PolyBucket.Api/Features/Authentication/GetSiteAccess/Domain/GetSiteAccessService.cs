using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

public class GetSiteAccessService(
    ISitePrivacyState sitePrivacyState,
    ISiteModelUploadPolicy uploadPolicy,
    ISiteModelDownloadPolicy downloadPolicy,
    IModelMetadataPolicy metadataPolicy,
    ISiteModelFeaturePolicy featurePolicy,
    ISiteModelVersioningPolicy versioningPolicy,
    ISiteModelAdvancedPolicy advancedPolicy) : IGetSiteAccessService
{
    private readonly ISitePrivacyState _sitePrivacyState = sitePrivacyState;
    private readonly ISiteModelUploadPolicy _uploadPolicy = uploadPolicy;
    private readonly ISiteModelDownloadPolicy _downloadPolicy = downloadPolicy;
    private readonly IModelMetadataPolicy _metadataPolicy = metadataPolicy;
    private readonly ISiteModelFeaturePolicy _featurePolicy = featurePolicy;
    private readonly ISiteModelVersioningPolicy _versioningPolicy = versioningPolicy;
    private readonly ISiteModelAdvancedPolicy _advancedPolicy = advancedPolicy;

    public async Task<GetSiteAccessResponse> GetSiteAccessAsync(CancellationToken cancellationToken)
    {
        var allowPublicBrowsing = await _sitePrivacyState.IsPublicBrowsingAllowedAsync(cancellationToken);
        var allowAnonymousModelUpload = await _uploadPolicy.IsAnonymousModelUploadAllowedAsync(cancellationToken);
        var requireLoginForUpload = await _uploadPolicy.IsLoginRequiredForUnauthenticatedUploadAsync(cancellationToken);
        var allowAnonymousModelDownload = await _downloadPolicy.IsAnonymousModelDownloadAllowedAsync(cancellationToken);
        var modelDownloadsEnabled = await _downloadPolicy.AreModelDownloadsEnabledAsync(cancellationToken);
        var modelVersioningEnabled = await _versioningPolicy.IsModelVersioningEnabledAsync(cancellationToken);
        var defaultPrivacy = await _uploadPolicy.GetDefaultModelPrivacyAsync(cancellationToken);
        var metadata = await _metadataPolicy.GetSnapshotAsync(cancellationToken);
        var commentsAccess = await _featurePolicy.GetCommentsAccessLevelAsync(cancellationToken);
        var collectionsAccess = await _featurePolicy.GetCollectionsAccessLevelAsync(cancellationToken);
        var advanced = await _advancedPolicy.GetSnapshotAsync(cancellationToken);

        return new GetSiteAccessResponse
        {
            AllowPublicBrowsing = allowPublicBrowsing,
            RequireLoginForUpload = requireLoginForUpload,
            AllowAnonymousModelUpload = allowAnonymousModelUpload,
            AllowAnonymousModelDownload = allowAnonymousModelDownload,
            ModelDownloadsEnabled = modelDownloadsEnabled,
            ModelVersioningEnabled = modelVersioningEnabled,
            ModelSharingEnabled = advanced.EnableModelSharing,
            ModelReportingEnabled = advanced.EnableModelReporting,
            ModelCommentsAccess = commentsAccess.ToString(),
            ModelCollectionsAccess = collectionsAccess.ToString(),
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
                RequireCategorySelection = metadata.RequireCategorySelection,
                AllowModelRemixing = metadata.AllowModelRemixing,
                RequireRemixAttribution = metadata.RequireRemixAttribution
            }
        };
    }
}