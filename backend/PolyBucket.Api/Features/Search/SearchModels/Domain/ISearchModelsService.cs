using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.SearchModels.Repository;

namespace PolyBucket.Api.Features.Search.SearchModels.Domain;

public interface ISearchModelsService
{
    Task<SearchResponse> SearchModelsAsync(SearchQuery query, CancellationToken cancellationToken = default);
}

public class SearchModelsService(ISearchModelsRepository repository) : ISearchModelsService
{
    public Task<SearchResponse> SearchModelsAsync(SearchQuery query, CancellationToken cancellationToken = default) =>
        repository.SearchModelsAsync(query, cancellationToken);
}
