using MediatR;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Domain
{
    public class CreateCategoryCommand : IRequest<CreateCategoryResponse>
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters")]
        public string Name { get; set; } = string.Empty;
    }
}
