namespace PolyBucket.Api.Features.Federation.ImportFederatedModel.Repository;

public interface IImportFederatedModelRepository
{
    Task<Model> ImportModelAsync(string instanceId, string remoteModelId, CancellationToken cancellationToken = default);
}
