namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISiteModelAdvancedPolicy
{
    Task<SiteModelAdvancedPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    void Invalidate();
}