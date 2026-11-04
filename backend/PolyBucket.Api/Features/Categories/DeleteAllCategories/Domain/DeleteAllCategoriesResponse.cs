namespace PolyBucket.Api.Features.Categories.DeleteAllCategories.Domain
{
    public class DeleteAllCategoriesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DeletedCount { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
