namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

public interface IGetSiteAccessService
{
    Task<GetSiteAccessResponse> GetSiteAccessAsync(CancellationToken cancellationToken);
}

public class GetSiteAccessResponse
{
    public bool AllowPublicBrowsing { get; set; }
}
