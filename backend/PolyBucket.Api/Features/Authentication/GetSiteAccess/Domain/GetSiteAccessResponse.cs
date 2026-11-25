namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

public interface IGetSiteAccessService
{
    Task<GetSiteAccessResponse> GetSiteAccessAsync(CancellationToken cancellationToken);
}

public class GetSiteAccessResponse
{
    public string SiteName { get; set; } = "PolyBucket";

    public bool AllowPublicBrowsing { get; set; }

    public bool RequireLoginForUpload { get; set; }

    public bool AllowAnonymousModelUpload { get; set; }

    public bool AllowAnonymousModelDownload { get; set; }

    public bool ModelDownloadsEnabled { get; set; } = true;

    public bool ModelVersioningEnabled { get; set; } = true;

    public bool ModelSharingEnabled { get; set; } = true;

    public bool ModelReportingEnabled { get; set; } = true;

    public string ModelCommentsAccess { get; set; } = "Full";

    public string ModelCollectionsAccess { get; set; } = "Full";

    public string DefaultModelPrivacy { get; set; } = "Public";

    public ModelMetadataRulesDto ModelMetadataRules { get; set; } = new();
}