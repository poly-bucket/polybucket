using PolyBucket.Api.Features.Search.Domain;

namespace PolyBucket.Api.Features.Search.Search.Domain;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
}

public class SearchService(Repository.ISearchRepository repository) : ISearchService
{
    public Task<SearchResponse> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default) =>
        repository.SearchAsync(query, cancellationToken);
}
