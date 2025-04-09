using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.Queries;
// using PolyBucket.Core.Features.Users.Queries;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Queries
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class GetUserByIdController : ControllerBase
    {
        private readonly GetUserByIdQueryHandler _getUserByIdQueryHandler;
        private readonly ILogger<GetUserByIdController> _logger;

        public GetUserByIdController(GetUserByIdQueryHandler getUserByIdQueryHandler, ILogger<GetUserByIdController> logger)
        {
            _getUserByIdQueryHandler = getUserByIdQueryHandler;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(GetUserByIdResponse))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        public async Task<ActionResult<GetUserByIdResponse>> GetUserById([FromRoute] Guid id)
        {
            try
            {
                var query = new GetUserByIdQuery { Id = id };
                var user = await _getUserByIdQueryHandler.Handle(query);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the user" });
            }
        }
    }
} 