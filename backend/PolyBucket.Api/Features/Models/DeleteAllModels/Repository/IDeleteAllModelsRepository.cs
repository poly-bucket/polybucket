namespace PolyBucket.Api.Features.Models.DeleteAllModels.Repository;

public interface IDeleteAllModelsRepository
{
    Task<int> DeleteAllModelsAndReturnCountAsync(CancellationToken cancellationToken = default);
}
