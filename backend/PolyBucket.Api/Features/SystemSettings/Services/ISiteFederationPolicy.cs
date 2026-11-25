namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISiteFederationPolicy
{
    Task<SiteFederationPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    void Invalidate();
}