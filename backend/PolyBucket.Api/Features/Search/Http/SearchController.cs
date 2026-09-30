using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Search.Http
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchRepository _searchRepository;

        public SearchController(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        /// <summary>
        /// Search for models, users, and collections
        /// </summary>
        /// <remarks>
        /// Matching and pagination run in the database. When the pg_trgm extension is installed, names tolerate typos
        /// and results are ordered by trigram similarity; otherwise matching falls back to case-insensitive substrings.
        /// For type All, each result type is paginated independently with the same page and page size, and
        /// <c>counts</c> reports the total matches per type.
        /// </remarks>
        /// <param name="query">Search query</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="type">Search type - All, Models, Users, or Collections (default: All)</param>
        /// <param name="category">Filter models by category name (optional)</param>
        /// <param name="sortBy">Sort by relevance, createdAt, downloads, or likes (default: relevance)</param>
        /// <param name="sortDescending">Sort in descending order; ignored for relevance (default: false)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Search results</returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(SearchResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<SearchResponse>> Search(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] SearchType type = SearchType.All,
            [FromQuery] string? category = null,
            [FromQuery] string sortBy = "relevance",
            [FromQuery] bool sortDescending = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "Search query is required" });
            }

            if (query.Length > 200)
            {
                return BadRequest(new { message = "Search query must be 200 characters or fewer" });
            }

            if (page < 1)
            {
                return BadRequest(new { message = "Page must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Page size must be between 1 and 100" });
            }

            try
            {
                var searchQuery = new SearchQuery
                {
                    Query = query.Trim(),
                    Page = page,
                    PageSize = pageSize,
                    Type = type,
                    Category = category,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var results = await _searchRepository.SearchAsync(searchQuery, cancellationToken);
                return Ok(results);
            }
            catch (System.Exception ex) when (ex is not System.OperationCanceledException)
            {
                return StatusCode(500, new { message = "An error occurred while searching" });
            }
        }
    }
}
