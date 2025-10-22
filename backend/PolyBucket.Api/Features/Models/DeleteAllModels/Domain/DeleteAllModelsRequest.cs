using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Models.DeleteAllModels.Domain
{
    public class DeleteAllModelsRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string AdminPassword { get; set; } = string.Empty;
    }
}
