using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Domain
{
    public class DeleteAllFileSettingsRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string AdminPassword { get; set; } = string.Empty;
    }
}
