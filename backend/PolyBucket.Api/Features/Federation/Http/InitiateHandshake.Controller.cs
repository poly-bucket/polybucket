using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Domain;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Handles initiating a federation handshake with a remote instance
    /// </summary>
    [ApiController]
    [Route("api/federation/handshake")]
    [AllowAnonymous] // Allow anonymous since this is called by remote instances
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation Handshake")]
    public class InitiateHandshakeController(
        PolyBucketDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration) : ControllerBase
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Initiate a federation handshake with a remote instance
        /// </summary>
        /// <param name="request">The handshake initiation request</param>
        /// <remarks>
        /// This endpoint is called BY a remote instance TO this instance when they want to initiate federation.
        /// 
        /// Step 1 of 6 in the federation handshake process:
        /// - Remote instance sends connection request with their instance details
        /// - This instance checks if it's accepting new federations
        /// - Responds with acceptance or rejection
        /// 
        /// If accepted, returns acceptance status and proceeds to token exchange.
        /// This is an unauthenticated endpoint as the remote instance doesn't have tokens yet.
        /// </remarks>
        /// <response code="200">Handshake accepted, proceed to token exchange</response>
        /// <response code="400">Bad request - validation failed</response>
        /// <response code="403">Not accepting new federations</response>
        /// <response code="422">Instance verification failed</response>
        [HttpPost("initiate")]
        [ProducesResponseType(typeof(InitiateHandshakeResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<ActionResult<InitiateHandshakeResponse>> InitiateHandshake(
            [FromBody] InitiateHandshakeRequest request)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.InitiatorUrl))
            {
                return BadRequest("Initiator URL is required");
            }

            if (string.IsNullOrWhiteSpace(request.InitiatorName))
            {
                return BadRequest("Initiator name is required");
            }

            if (!Uri.TryCreate(request.InitiatorUrl, UriKind.Absolute, out var initiatorUri) || 
                initiatorUri.Scheme != Uri.UriSchemeHttps)
            {
                return BadRequest("Initiator URL must be a valid HTTPS URL");
            }

            // Check if we're accepting new federations
            // TODO: Add global federation settings check here
            // For now, we'll accept all handshakes

            // Verify the remote instance is a legitimate PolyBucket instance
            var isValid = await VerifyRemoteInstanceAsync(request.InitiatorUrl);
            if (!isValid)
            {
                return UnprocessableEntity("Could not verify remote instance as a valid PolyBucket instance");
            }

            // Create handshake record
            var handshake = new FederationHandshake
            {
                Id = Guid.NewGuid(),
                InitiatorUrl = request.InitiatorUrl.TrimEnd('/'),
                ResponderUrl = GetThisInstanceUrl(),
                Status = HandshakeStatus.Accepted,
                HandshakeToken = Guid.NewGuid().ToString(), // Temporary token for this handshake
                ExpiresAt = DateTime.UtcNow.AddHours(24), // Handshake expires in 24 hours
                CreatedById = Guid.Empty, // System-initiated
                CreatedAt = DateTime.UtcNow
            };

            _context.FederationHandshakes.Add(handshake);
            await _context.SaveChangesAsync();

            return Ok(new InitiateHandshakeResponse
            {
                HandshakeId = handshake.Id,
                Status = "Accepted",
                ResponderUrl = handshake.ResponderUrl,
                ResponderName = _configuration["Instance:Name"] ?? "PolyBucket Instance",
                HandshakeToken = handshake.HandshakeToken,
                ExpiresAt = handshake.ExpiresAt,
                Message = "Handshake accepted. Proceed to token exchange."
            });
        }

        private async Task<bool> VerifyRemoteInstanceAsync(string url)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                // Try to reach the health endpoint
                var healthUrl = $"{url.TrimEnd('/')}/api/health";
                var response = await httpClient.GetAsync(healthUrl);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private string GetThisInstanceUrl()
        {
            // Try to get from configuration first
            var configuredUrl = _configuration["Instance:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                return configuredUrl.TrimEnd('/');
            }

            // Fall back to request URL
            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
        }
    }

    public class InitiateHandshakeRequest
    {
        /// <summary>
        /// The URL of the instance initiating the handshake
        /// </summary>
        public string InitiatorUrl { get; set; } = string.Empty;

        /// <summary>
        /// The name of the instance initiating the handshake
        /// </summary>
        public string InitiatorName { get; set; } = string.Empty;

        /// <summary>
        /// Version of PolyBucket the initiator is running
        /// </summary>
        public string? Version { get; set; }
    }

    public class InitiateHandshakeResponse
    {
        /// <summary>
        /// The unique ID for this handshake
        /// </summary>
        public Guid HandshakeId { get; set; }

        /// <summary>
        /// Status of the handshake (Accepted or Rejected)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The URL of this responder instance
        /// </summary>
        public string ResponderUrl { get; set; } = string.Empty;

        /// <summary>
        /// The name of this responder instance
        /// </summary>
        public string ResponderName { get; set; } = string.Empty;

        /// <summary>
        /// Temporary token for this handshake session
        /// </summary>
        public string? HandshakeToken { get; set; }

        /// <summary>
        /// When the handshake expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Additional message
        /// </summary>
        public string? Message { get; set; }
    }
}

