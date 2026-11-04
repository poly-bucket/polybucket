namespace PolyBucket.Api.Features.Collections.DeleteAllCollections.Domain
{
    public class DeleteAllCollectionsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DeletedCount { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
