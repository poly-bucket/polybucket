using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Federation.Services;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Handles token exchange between federated instances
    /// </summary>
    [ApiController]
    [Route("api/federation/handshake")]
    [AllowAnonymous] // Allow anonymous since tokens are being exchanged
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation Handshake")]
    public class ExchangeTokenController(
        PolyBucketDbContext context,
        IFederationTokenService tokenService,
        IConfiguration configuration) : ControllerBase
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IFederationTokenService _tokenService = tokenService;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Exchange federation tokens between instances
        /// </summary>
        /// <param name="request">The token exchange request</param>
        /// <remarks>
        /// This endpoint handles Steps 2 & 3 of the federation handshake:
        /// 
        /// Step 2: Instance B generates and sends a token to Instance A
        /// Step 3: Instance A validates the token and sends its own token back to Instance B
        /// 
        /// This endpoint can be called in both directions:
        /// - By Instance A to receive Instance B's token (Step 2)
        /// - By Instance B to receive Instance A's token (Step 3)
        /// 
        /// Requires the handshake token from the initiation step for authentication.
        /// </remarks>
        /// <response code="200">Token exchange successful</response>
        /// <response code="400">Bad request - validation failed</response>
        /// <response code="401">Invalid or expired handshake token</response>
        /// <response code="404">Handshake not found</response>
        [HttpPost("exchange-token")]
        [ProducesResponseType(typeof(ExchangeTokenResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ExchangeTokenResponse>> ExchangeToken(
            [FromBody] ExchangeTokenRequest request)
        {
            // Validate request
            if (request.HandshakeId == Guid.Empty)
            {
                return BadRequest("Handshake ID is required");
            }

            if (string.IsNullOrWhiteSpace(request.HandshakeToken))
            {
                return BadRequest("Handshake token is required");
            }

            // Find the handshake
            var handshake = await _context.FederationHandshakes
                .FirstOrDefaultAsync(h => h.Id == request.HandshakeId);

            if (handshake == null)
            {
                return NotFound("Handshake not found");
            }

            // Verify handshake token
            if (handshake.HandshakeToken != request.HandshakeToken)
            {
                return Unauthorized("Invalid handshake token");
            }

            // Check if handshake has expired
            if (handshake.ExpiresAt < DateTime.UtcNow)
            {
                handshake.Status = HandshakeStatus.Expired;
                await _context.SaveChangesAsync();
                return Unauthorized("Handshake has expired");
            }

            // Find or create the federated instance
            var instance = await GetOrCreateFederatedInstanceAsync(request, handshake);

            // Generate our token for the remote instance
            var thisInstanceId = instance.Id;
            var remoteInstanceId = instance.Id; // Same instance in this case
            var thisInstanceUrl = GetThisInstanceUrl();
            
            var federationToken = await _tokenService.GenerateTokenAsync(
                thisInstanceId,
                remoteInstanceId,
                thisInstanceUrl,
                expirationDays: 90
            );

            // Encrypt and store our token
            var encryptedOurToken = await _tokenService.EncryptTokenAsync(federationToken);
            instance.OurToken = encryptedOurToken;
            instance.OurTokenExpiry = DateTime.UtcNow.AddDays(90);

            // If they provided their token, store it
            if (!string.IsNullOrWhiteSpace(request.TheirToken))
            {
                // Validate their token
                var isValid = await _tokenService.ValidateTokenAsync(request.TheirToken, thisInstanceId);
                if (!isValid)
                {
                    return BadRequest("Provided token is invalid");
                }

                // Encrypt and store their token
                var encryptedTheirToken = await _tokenService.EncryptTokenAsync(request.TheirToken);
                instance.TheirToken = encryptedTheirToken;
                instance.TheirTokenExpiry = await _tokenService.GetTokenExpirationAsync(request.TheirToken);

                // Both tokens exchanged - update handshake status
                handshake.Status = HandshakeStatus.TokenExchanged;
                instance.Status = FederationStatus.Active;
            }
            else
            {
                // Only our token sent - waiting for their token
                handshake.Status = HandshakeStatus.Accepted;
            }

            await _context.SaveChangesAsync();

            return Ok(new ExchangeTokenResponse
            {
                OurToken = federationToken, // Send unencrypted token to remote instance
                TokenExpiry = instance.OurTokenExpiry!.Value,
                Status = handshake.Status.ToString(),
                InstanceId = instance.Id,
                Message = handshake.Status == HandshakeStatus.TokenExchanged 
                    ? "Token exchange complete. Proceed to catalog sharing." 
                    : "Token sent. Awaiting remote instance token."
            });
        }

        private async Task<FederatedInstance> GetOrCreateFederatedInstanceAsync(
            ExchangeTokenRequest request,
            FederationHandshake handshake)
        {
            // Determine if we are the initiator or responder
            var thisInstanceUrl = GetThisInstanceUrl();
            var isInitiator = handshake.InitiatorUrl != thisInstanceUrl;

            var remoteUrl = isInitiator ? handshake.ResponderUrl : handshake.InitiatorUrl;
            var remoteName = request.InstanceName ?? "Remote Instance";

            // Find existing instance by URL
            var instance = await _context.FederatedInstances
                .FirstOrDefaultAsync(i => i.BaseUrl == remoteUrl);

            if (instance == null)
            {
                // Create new instance
                instance = new FederatedInstance
                {
                    Id = Guid.NewGuid(),
                    Name = remoteName,
                    BaseUrl = remoteUrl,
                    Status = FederationStatus.Pending,
                    IsEnabled = true,
                    CreatedById = Guid.Empty, // System-created
                    CreatedAt = DateTime.UtcNow
                };

                _context.FederatedInstances.Add(instance);

                // Link handshake to instance
                if (isInitiator)
                {
                    handshake.ResponderInstanceId = instance.Id;
                }
                else
                {
                    handshake.InitiatorInstanceId = instance.Id;
                }
            }

            return instance;
        }

        private string GetThisInstanceUrl()
        {
            var configuredUrl = _configuration["Instance:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                return configuredUrl.TrimEnd('/');
            }

            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
        }
    }

    public class ExchangeTokenRequest
    {
        /// <summary>
        /// The handshake ID from the initiation step
        /// </summary>
        public Guid HandshakeId { get; set; }

        /// <summary>
        /// The temporary handshake token for authentication
        /// </summary>
        public string HandshakeToken { get; set; } = string.Empty;

        /// <summary>
        /// The instance name
        /// </summary>
        public string? InstanceName { get; set; }

        /// <summary>
        /// The federation token from the remote instance (if they're sending theirs)
        /// </summary>
        public string? TheirToken { get; set; }
    }

    public class ExchangeTokenResponse
    {
        /// <summary>
        /// The federation token for the remote instance to use
        /// </summary>
        public string OurToken { get; set; } = string.Empty;

        /// <summary>
        /// When the token expires
        /// </summary>
        public DateTime TokenExpiry { get; set; }

        /// <summary>
        /// Current handshake status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The instance ID created/found
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Additional message
        /// </summary>
        public string? Message { get; set; }
    }
}

