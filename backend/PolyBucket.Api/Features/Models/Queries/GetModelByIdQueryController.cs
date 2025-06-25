using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.Queries
{
    [ApiController]
    [Route("api/models")]
    public class GetModelByIdQueryController : ControllerBase
    {
        private readonly GetModelByIdQueryHandler _handler;
        private readonly ILogger<GetModelByIdQueryController> _logger;

        public GetModelByIdQueryController(GetModelByIdQueryHandler handler, ILogger<GetModelByIdQueryController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetModelByIdResponse>> GetModel(Guid id)
        {
            try
            {
                var request = new GetModelByIdRequest { Id = id };
                var response = await _handler.ExecuteAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving model with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the model" });
            }
        }
    }
} 