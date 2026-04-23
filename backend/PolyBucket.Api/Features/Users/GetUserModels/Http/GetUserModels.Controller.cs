using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserModels.Domain;

namespace PolyBucket.Api.Features.Users.GetUserModels.Http;

[ApiController]
[Route("api/users/models")]
public class GetUserModelsController : ControllerBase
{
    private readonly IGetUserModelsService _getUserModelsService;
    private readonly ILogger<GetUserModelsController> _logger;

    public GetUserModelsController(IGetUserModelsService getUserModelsService, ILogger<GetUserModelsController> logger)
    {
        _getUserModelsService = getUserModelsService;
        _logger = logger;
    }

    [HttpGet("{username}/models/public")]
    [ProducesResponseType(200, Type = typeof(GetUserModelsResult))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserModelsResult>> GetUserPublicModels(
        [FromRoute] string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserModelsQuery
            {
                Username = username,
                Page = page,
                PageSize = pageSize,
                IncludePrivate = false
            };

            var response = await _getUserModelsService.GetUserPublicModelsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User models not found for username {Username}", username);
            return NotFound(new { message = "User models not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user models for username {Username}", username);
            return StatusCode(500, new { message = "An error occurred while retrieving user models" });
        }
    }
}
