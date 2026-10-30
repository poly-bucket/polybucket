namespace PolyBucket.Api.Features.Federation.GetFederatedModels.Http;

public class FederatedModelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? RemoteInstanceId { get; set; }
        public string? RemoteModelId { get; set; }
        public Guid? RemoteAuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public int Downloads { get; set; }
        public int Likes { get; set; }
        public DateTime? LastFederationSync { get; set; }
        public DateTime CreatedAt { get; set; }
    }
