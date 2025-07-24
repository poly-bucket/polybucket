using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Models.UpdateModelVersion.Http
{
    public class UpdateModelVersionRequest
    {
        [StringLength(255)]
        public string? Name { get; set; }
        
        [StringLength(2000)]
        public string? Notes { get; set; }
    }
} 