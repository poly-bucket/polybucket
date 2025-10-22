using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Repository;
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
        /// <param name="query">Search query</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="type">Search type - All, Models, Users, or Collections (default: All)</param>
        /// <param name="category">Filter by category (optional)</param>
        /// <param name="sortBy">Sort by field (default: relevance)</param>
        /// <param name="sortDescending">Sort in descending order (default: false)</param>
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
            [FromQuery] bool sortDescending = false)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "Search query is required" });
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

                var results = await _searchRepository.SearchAsync(searchQuery);
                return Ok(results);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching", error = ex.Message });
            }
        }
    }
}
