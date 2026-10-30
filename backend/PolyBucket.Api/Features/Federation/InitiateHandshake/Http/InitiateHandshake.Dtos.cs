namespace PolyBucket.Api.Features.Federation.InitiateHandshake.Http;

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
