namespace PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Http;

public class UpdateFederatedInstanceRequest
    {
        public string? Name { get; set; }
        public string? BaseUrl { get; set; }
        public string? Description { get; set; }
        public string? PublicKey { get; set; }
        public string? SharedSecret { get; set; }
        public bool? IsEnabled { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateFederatedInstanceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
