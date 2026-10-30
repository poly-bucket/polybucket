namespace PolyBucket.Api.Features.Federation.ExchangeToken.Http;

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
