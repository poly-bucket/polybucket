using System;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Domain
{
    public class CreateCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
    }
}
