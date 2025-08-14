using System.Collections.Generic;

namespace PolyBucket.Api.Features.Categories.GetCategories.Domain
{
    public class GetCategoriesResponse
    {
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class CategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
