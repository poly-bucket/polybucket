using System;

namespace PolyBucket.Api.Features.Categories.UpdateCategory.Domain
{
    public class UpdateCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public Guid UpdatedById { get; set; }
    }
}
