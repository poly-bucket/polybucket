using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Collections.DeleteAllCollections.Domain
{
    public class DeleteAllCollectionsRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string AdminPassword { get; set; } = string.Empty;
    }
}
