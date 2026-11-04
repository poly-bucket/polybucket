using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Categories.DeleteAllCategories.Domain
{
    public class DeleteAllCategoriesRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string AdminPassword { get; set; } = string.Empty;
    }
}
