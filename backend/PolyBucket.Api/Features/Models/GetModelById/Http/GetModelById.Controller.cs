using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GetModelById.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Features.Models.GetModelById.Http
{
    [ApiController]
    [Route("api/models")]
    public class GetModelByIdController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetModelByIdController> _logger;

        public GetModelByIdController(IMediator mediator, ILogger<GetModelByIdController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetModelByIdResponse>> GetModel(Guid id)
        {
            _logger.LogDebug("GetModelByIdController.GetModel called with ID: {Id}", id);
            
            try
            {
                var query = new GetModelByIdQuery { Id = id };
                _logger.LogDebug("Sending GetModelByIdQuery to MediatR");
                var response = await _mediator.Send(query);
                _logger.LogDebug("Received response from MediatR");
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Model not found for ID: {Id}", id);
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to model ID: {Id}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetModelByIdController.GetModel for ID: {Id}", id);
                throw;
            }
        }
    }
} 