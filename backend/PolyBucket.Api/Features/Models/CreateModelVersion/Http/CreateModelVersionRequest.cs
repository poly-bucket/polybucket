using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Models.CreateModelVersion.Http
{
    public class CreateModelVersionRequest
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Notes { get; set; }
        
        public string? ThumbnailFileId { get; set; }
        
        [Required]
        public IFormFile[] Files { get; set; } = Array.Empty<IFormFile>();
    }
} 