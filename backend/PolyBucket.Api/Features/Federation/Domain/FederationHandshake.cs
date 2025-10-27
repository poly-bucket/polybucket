using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Federation.Domain
{
    /// <summary>
    /// Represents a federation handshake between two PolyBucket instances
    /// </summary>
    public class FederationHandshake : Auditable
    {
        public new Guid Id { get; set; }
        
        /// <summary>
        /// The federated instance ID if we initiated the handshake
        /// </summary>
        public Guid? InitiatorInstanceId { get; set; }
        
        /// <summary>
        /// The federated instance ID if they initiated the handshake
        /// </summary>
        public Guid? ResponderInstanceId { get; set; }
        
        /// <summary>
        /// URL of the instance that initiated the handshake
        /// </summary>
        public string InitiatorUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// URL of the instance responding to the handshake
        /// </summary>
        public string ResponderUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Current status of the handshake
        /// </summary>
        public HandshakeStatus Status { get; set; } = HandshakeStatus.Initiated;
        
        /// <summary>
        /// Temporary token used during handshake process
        /// </summary>
        public string? HandshakeToken { get; set; }
        
        /// <summary>
        /// Error message if handshake failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// When the handshake was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// When the handshake expires (24 hours from creation)
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Navigation property to the initiator instance
        /// </summary>
        public virtual FederatedInstance? InitiatorInstance { get; set; }
        
        /// <summary>
        /// Navigation property to the responder instance
        /// </summary>
        public virtual FederatedInstance? ResponderInstance { get; set; }
    }
}
