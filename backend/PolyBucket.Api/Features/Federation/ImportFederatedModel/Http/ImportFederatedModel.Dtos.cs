namespace PolyBucket.Api.Features.Federation.ImportFederatedModel.Http;

public class ImportFederatedModelRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string RemoteModelId { get; set; } = string.Empty;
    }

    public class ImportFederatedModelResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? RemoteInstanceId { get; set; }
        public string? RemoteModelId { get; set; }
        public bool IsFederated { get; set; }
        public DateTime? LastFederationSync { get; set; }
        public DateTime CreatedAt { get; set; }
    }
