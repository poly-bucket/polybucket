using PolyBucket.Api.Features.Search.Domain;

namespace PolyBucket.Api.Features.Search.SearchModels.Repository;

public interface ISearchModelsRepository
{
    Task<SearchResponse> SearchModelsAsync(SearchQuery query, CancellationToken cancellationToken = default);
}
