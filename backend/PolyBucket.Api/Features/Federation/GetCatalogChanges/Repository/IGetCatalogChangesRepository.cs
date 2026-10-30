using PolyBucket.Api.Features.Federation.GetCatalogChanges.Http;

namespace PolyBucket.Api.Features.Federation.GetCatalogChanges.Repository;

public interface IGetCatalogChangesRepository
{
    Task<CatalogChangesResponse> GetChangesAsync(DateTime since, CancellationToken cancellationToken = default);
}
