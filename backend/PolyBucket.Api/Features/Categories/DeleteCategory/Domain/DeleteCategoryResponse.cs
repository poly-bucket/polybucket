using System;

namespace PolyBucket.Api.Features.Categories.DeleteCategory.Domain
{
    public class DeleteCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DeletedAt { get; set; }
        public Guid DeletedById { get; set; }
    }
}
