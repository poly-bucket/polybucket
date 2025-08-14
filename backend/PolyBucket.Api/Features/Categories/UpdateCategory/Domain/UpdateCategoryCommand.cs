using MediatR;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Categories.UpdateCategory.Domain
{
    public class UpdateCategoryCommand : IRequest<UpdateCategoryResponse>
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters")]
        public string Name { get; set; } = string.Empty;
    }
}
