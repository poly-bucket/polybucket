using PolyBucket.Api.Common.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Models.UpdateModel.Http
{
    public class UpdateModelRequest
    {
        [StringLength(255)]
        public string? Name { get; set; }
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        public LicenseTypes? License { get; set; }
        
        public PrivacySettings? Privacy { get; set; }
        
        public bool? AIGenerated { get; set; }
        
        public bool? WIP { get; set; }
        
        public bool? NSFW { get; set; }
        
        public bool? IsRemix { get; set; }
        
        [StringLength(500)]
        public string? RemixUrl { get; set; }
        
        public bool? IsFeatured { get; set; }
    }
} 