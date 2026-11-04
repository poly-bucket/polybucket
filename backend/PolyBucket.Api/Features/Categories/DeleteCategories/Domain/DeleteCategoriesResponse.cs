using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Categories.DeleteCategories.Domain
{
    public class DeleteCategoriesResponse
    {
        public int DeletedCount { get; set; }
        public int FailedCount { get; set; }
        public List<DeleteCategoryItemResult> Results { get; set; } = new();
    }

    public class DeleteCategoryItemResult
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
