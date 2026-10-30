namespace PolyBucket.Api.Features.Federation.GetFederationHealth.Http;

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
