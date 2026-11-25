using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISiteModelVersioningPolicy
{
    Task<bool> IsModelVersioningEnabledAsync(CancellationToken cancellationToken = default);

    Task<SiteModelVersioningPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    Task<bool> CanViewVersionHistoryWhenDisabledAsync(
        ClaimsPrincipal user,
        Guid modelAuthorId,
        CancellationToken cancellationToken = default);

    void Invalidate();
}

public sealed record SiteModelVersioningPolicySnapshot(bool EnableModelVersioning)
{
    public static SiteModelVersioningPolicySnapshot FailClosed { get; } = new(false);
}