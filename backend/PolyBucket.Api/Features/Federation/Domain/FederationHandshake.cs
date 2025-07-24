using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Federation.Domain
{
    public class FederationHandshake : Auditable
    {
        public new Guid Id { get; set; }
        
        public Guid FederatedInstanceId { get; set; }
        public FederatedInstance FederatedInstance { get; set; } = null!;
        
        public HandshakeDirection Direction { get; set; }
        public HandshakeStatus Status { get; set; } = HandshakeStatus.Initiated;
        public string? Challenge { get; set; }
        public string? Response { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Timing information
        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        
        // Protocol information
        public string ProtocolVersion { get; set; } = "1.0";
        public string? ClientVersion { get; set; }
        public string? ServerVersion { get; set; }
        
        // Cryptographic information
        public string? TempPublicKey { get; set; }
        public string? KeyExchangeData { get; set; }
        public string? SignatureData { get; set; }
        
        // Connection metadata
        public string? RemoteIpAddress { get; set; }
        public string? UserAgent { get; set; }
        public int AttemptNumber { get; set; } = 1;
        public int MaxAttempts { get; set; } = 3;
        
        // Instance information exchanged during handshake
        public string? RemoteInstanceName { get; set; }
        public string? RemoteInstanceVersion { get; set; }
        public string? RemoteAdminContact { get; set; }
        public string? RemoteCapabilities { get; set; } // JSON array of supported features
    }

    public enum HandshakeDirection
    {
        Outgoing = 1, // We initiated the handshake
        Incoming = 2  // They initiated the handshake
    }

    public enum HandshakeStatus
    {
        Initiated = 0,
        ChallengeSet = 1,
        ChallengeReceived = 2,
        ResponseSent = 3,
        ResponseReceived = 4,
        KeysExchanged = 5,
        Completed = 6,
        Failed = 7,
        Expired = 8,
        Rejected = 9
    }
} 