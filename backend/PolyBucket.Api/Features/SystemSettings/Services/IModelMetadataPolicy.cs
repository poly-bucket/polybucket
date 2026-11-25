namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface IModelMetadataPolicy
{
    Task<ModelMetadataPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    void Invalidate();
}