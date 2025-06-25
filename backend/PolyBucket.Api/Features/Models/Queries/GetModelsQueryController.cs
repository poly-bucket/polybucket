using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Features.Models.Queries
{
    [ApiController]
    [Route("api/models")]
    public class GetModelsQueryController : ControllerBase
    {
        private readonly GetModelsQueryHandler _handler;
        private readonly ILogger<GetModelsQueryController> _logger;

        public GetModelsQueryController(GetModelsQueryHandler handler, ILogger<GetModelsQueryController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<GetModelsResponse>> GetModels([FromQuery] GetModelsRequest request)
        {
            try
            {
                var response = await _handler.ExecuteAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving models");
                return StatusCode(500, new { message = "An error occurred while retrieving models" });
            }
        }
    }
} 