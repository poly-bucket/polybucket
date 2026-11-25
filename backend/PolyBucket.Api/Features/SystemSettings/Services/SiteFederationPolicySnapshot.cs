namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed record SiteFederationPolicySnapshot(
    Guid SystemSetupId,
    bool EnableFederation,
    string InstanceName,
    string InstanceDescription,
    string AdminContact)
{
    public static SiteFederationPolicySnapshot Disabled { get; } = new(
        Guid.Empty,
        false,
        string.Empty,
        string.Empty,
        string.Empty);

    public bool IsEnabled => EnableFederation;

    public bool HasInstanceName => !string.IsNullOrWhiteSpace(InstanceName);
}