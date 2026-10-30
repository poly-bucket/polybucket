using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.SearchModels.Domain;

namespace PolyBucket.Api.Features.Search.SearchModels.Http;

[ApiController]
[Route("api/search")]
public class SearchModelsController(ISearchModelsService searchModelsService) : ControllerBase
{
    /// <summary>
    /// Search public models only.
    /// </summary>
    [HttpGet("models")]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResponse>> SearchModels(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
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
                Type = SearchType.Models,
                Category = category,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            return Ok(await searchModelsService.SearchModelsAsync(searchQuery, cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StatusCode(500, new { message = "An error occurred while searching models" });
        }
    }
}
