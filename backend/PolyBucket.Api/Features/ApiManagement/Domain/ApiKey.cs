using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.ApiManagement.Domain
{
    public class ApiKey : Auditable
    {
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string? Description { get; set; }
        public string[] Permissions { get; set; } = Array.Empty<string>();
        public int RateLimitPerHour { get; set; } = 1000;
        public int RateLimitPerDay { get; set; } = 10000;
        public long TotalRequests { get; set; } = 0;
        public DateTime? RevokedAt { get; set; }
        public string? RevokeReason { get; set; }
    }

    public class ApiKeyUsage : Auditable
    {
        public Guid ApiKeyId { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int ResponseCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
