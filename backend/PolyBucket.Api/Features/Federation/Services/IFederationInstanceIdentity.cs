namespace PolyBucket.Api.Features.Federation.Services;

public interface IFederationInstanceIdentity
{
    Task<FederationInstanceIdentitySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed record FederationInstanceIdentitySnapshot(
    Guid InstanceId,
    string InstanceName,
    string InstanceDescription,
    string AdminContact,
    string BaseUrl,
    bool IsReadyForPeering);