using PolyBucket.Api.Features.Federation.GetCatalog.Http;

namespace PolyBucket.Api.Features.Federation.GetCatalog.Repository;

public interface IGetCatalogRepository
{
    Task<(int TotalCount, List<CatalogModelDto> Models)> GetCatalogPageAsync(int page, int pageSize, DateTime? since, CancellationToken cancellationToken = default);
}
