namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

public interface IGetSiteAccessService
{
    Task<GetSiteAccessResponse> GetSiteAccessAsync(CancellationToken cancellationToken);
}

public class GetSiteAccessResponse
{
    public bool AllowPublicBrowsing { get; set; }

    public bool RequireLoginForUpload { get; set; }

    public bool AllowAnonymousModelUpload { get; set; }

  public string DefaultModelPrivacy { get; set; } = "Public";

    public ModelMetadataRulesDto ModelMetadataRules { get; set; } = new();
}