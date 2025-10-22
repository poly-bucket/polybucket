using PolyBucket.Api.Features.Search.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Search.Repository
{
    public interface ISearchRepository
    {
        Task<SearchResponse> SearchAsync(SearchQuery query);
    }
}
