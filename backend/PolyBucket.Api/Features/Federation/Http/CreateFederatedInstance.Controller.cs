using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Manages creation of new federated instances
    /// </summary>
    [ApiController]
    [Route("api/federation/instances")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class CreateFederatedInstanceController(
        IFederationRepository repository,
        IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly IFederationRepository _repository = repository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Create a new federated instance connection
        /// </summary>
        /// <param name="request">The federated instance creation request</param>
        /// <remarks>
        /// Creates a new connection to a federated PolyBucket instance. This initiates the federation handshake process.
        /// The instance will be created with a status of "Pending" until the handshake is completed.
        /// Requires admin permissions.
        /// </remarks>
        /// <response code="201">Federated instance created successfully</response>
        /// <response code="400">Bad request - validation failed</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="403">Forbidden - user lacks admin permissions</response>
        [HttpPost]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(CreateFederatedInstanceResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<CreateFederatedInstanceResponse>> CreateFederatedInstance(
            [FromBody] CreateFederatedInstanceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Instance name is required");
            }

            if (string.IsNullOrWhiteSpace(request.BaseUrl))
            {
                return BadRequest("Instance base URL is required");
            }

            if (!Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out _))
            {
                return BadRequest("Invalid base URL format");
            }

            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User is not authenticated");
            }

            var userId = Guid.Parse(userIdClaim);
            var now = DateTime.UtcNow;

            var instance = new FederatedInstance
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                BaseUrl = request.BaseUrl.TrimEnd('/'),
                Description = request.Description,
                PublicKey = request.PublicKey,
                SharedSecret = request.SharedSecret,
                Status = FederationStatus.Pending,
                IsEnabled = request.IsEnabled ?? true,
                CreatedById = userId,
                CreatedAt = now,
                UpdatedById = userId
            };

            var created = await _repository.AddFederatedInstanceAsync(instance);

            return CreatedAtAction(
                nameof(GetFederatedInstanceController.GetFederatedInstance),
                "GetFederatedInstance",
                new { id = created.Id },
                new CreateFederatedInstanceResponse
                {
                    Id = created.Id,
                    Name = created.Name,
                    BaseUrl = created.BaseUrl,
                    Status = created.Status.ToString(),
                    Description = created.Description,
                    IsEnabled = created.IsEnabled,
                    CreatedAt = created.CreatedAt
                });
        }
    }

    public class CreateFederatedInstanceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PublicKey { get; set; }
        public string? SharedSecret { get; set; }
        public bool? IsEnabled { get; set; }
    }

    public class CreateFederatedInstanceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

