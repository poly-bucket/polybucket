using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Manages health checks for federated instances
    /// </summary>
    [ApiController]
    [Route("api/federation/health")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class GetFederationHealthController(
        IFederationRepository repository,
        IHttpClientFactory httpClientFactory) : ControllerBase
    {
        private readonly IFederationRepository _repository = repository;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        /// <summary>
        /// Check the health status of a federated instance
        /// </summary>
        /// <param name="instanceId">The unique identifier of the federated instance</param>
        /// <remarks>
        /// Performs a health check on the specified federated instance by pinging its health endpoint.
        /// Returns connectivity status, response time, and any errors encountered.
        /// Uses a 10-second timeout for the health check request.
        /// Requires admin permissions.
        /// </remarks>
        /// <response code="200">Returns the health status of the federated instance</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="403">Forbidden - user lacks admin permissions</response>
        /// <response code="404">Federated instance not found</response>
        [HttpGet("{instanceId}")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(FederationHealthResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FederationHealthResponse>> GetFederationHealth(Guid instanceId)
        {
            var instance = await _repository.GetFederatedInstanceAsync(instanceId);
            
            if (instance == null)
            {
                return NotFound($"Federated instance with ID {instanceId} not found");
            }

            var healthStatus = await CheckInstanceHealthAsync(instance.BaseUrl);

            return Ok(new FederationHealthResponse
            {
                InstanceId = instance.Id,
                InstanceName = instance.Name,
                BaseUrl = instance.BaseUrl,
                Status = instance.Status.ToString(),
                IsEnabled = instance.IsEnabled,
                IsReachable = healthStatus.IsReachable,
                ResponseTimeMs = healthStatus.ResponseTimeMs,
                LastSyncAt = instance.LastSyncAt,
                ErrorMessage = healthStatus.ErrorMessage,
                CheckedAt = DateTime.UtcNow
            });
        }

        private async Task<HealthStatus> CheckInstanceHealthAsync(string baseUrl)
        {
            var startTime = DateTime.UtcNow;
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                var healthUrl = $"{baseUrl.TrimEnd('/')}/api/health";
                var response = await httpClient.GetAsync(healthUrl);
                var endTime = DateTime.UtcNow;
                var responseTime = (endTime - startTime).TotalMilliseconds;

                return new HealthStatus
                {
                    IsReachable = response.IsSuccessStatusCode,
                    ResponseTimeMs = (int)responseTime,
                    ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
                };
            }
            catch (HttpRequestException ex)
            {
                var endTime = DateTime.UtcNow;
                var responseTime = (endTime - startTime).TotalMilliseconds;

                return new HealthStatus
                {
                    IsReachable = false,
                    ResponseTimeMs = (int)responseTime,
                    ErrorMessage = $"Connection failed: {ex.Message}"
                };
            }
            catch (TaskCanceledException)
            {
                return new HealthStatus
                {
                    IsReachable = false,
                    ResponseTimeMs = 10000,
                    ErrorMessage = "Request timeout"
                };
            }
        }
    }

    public class FederationHealthResponse
    {
        public Guid InstanceId { get; set; }
        public string InstanceName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsReachable { get; set; }
        public int ResponseTimeMs { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CheckedAt { get; set; }
    }

    public class HealthStatus
    {
        public bool IsReachable { get; set; }
        public int ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

