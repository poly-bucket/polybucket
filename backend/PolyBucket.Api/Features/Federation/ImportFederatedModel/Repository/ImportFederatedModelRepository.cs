using PolyBucket.Api.Features.Federation.Services;

namespace PolyBucket.Api.Features.Federation.ImportFederatedModel.Repository;

public class ImportFederatedModelRepository(IFederationImportService importService) : IImportFederatedModelRepository
{
    public Task<Model> ImportModelAsync(string instanceId, string remoteModelId, CancellationToken cancellationToken = default) =>
        importService.ImportModelAsync(instanceId, remoteModelId);
}
