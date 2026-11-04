using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Categories.UpdateCategory.Domain
{
    public class UpdateCategoryCommand : IRequest<UpdateCategoryResponse>
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [RegularExpression(@"^$|^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$", ErrorMessage = "Color must be a valid hex color code (e.g., #3B82F6) or empty")]
        public string Color { get; set; } = string.Empty;
    }
}
