namespace PolyBucket.Api.Features.Models.UploadValidation.Domain;

public interface IModelUploadLimitsProvider
{
  Task<ModelUploadLimitsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}