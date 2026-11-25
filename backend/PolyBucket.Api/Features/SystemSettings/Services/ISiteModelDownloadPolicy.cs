namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISiteModelDownloadPolicy
{
    Task<bool> IsAnonymousModelDownloadAllowedAsync(CancellationToken cancellationToken = default);

    Task<bool> AreModelDownloadsEnabledAsync(CancellationToken cancellationToken = default);

    Task<SiteModelDownloadPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    void Invalidate();
}

public sealed record SiteModelDownloadPolicySnapshot(
    bool AllowAnonDownloads,
    bool EnableModelDownloads)
{
    public static SiteModelDownloadPolicySnapshot FailClosed { get; } = new(false, false);
}