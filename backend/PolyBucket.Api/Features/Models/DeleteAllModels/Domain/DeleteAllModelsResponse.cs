namespace PolyBucket.Api.Features.Models.DeleteAllModels.Domain
{
    public class DeleteAllModelsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DeletedCount { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
