using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Search.Domain;

namespace PolyBucket.Api.Features.Search.Search.Http;

[ApiController]
[Route("api/search")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    /// <summary>
    /// Search for models, users, and collections
    /// </summary>
    /// <remarks>
    /// Matching and pagination run in the database. When the pg_trgm extension is installed, names tolerate typos
    /// and results are ordered by trigram similarity; otherwise matching falls back to case-insensitive substrings.
    /// For type All, each result type is paginated independently with the same page and page size, and
    /// <c>counts</c> reports the total matches per type.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

            return Ok(await searchService.SearchAsync(searchQuery, cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StatusCode(500, new { message = "An error occurred while searching" });
        }
    }
}
