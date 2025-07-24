using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Models.CreateModel.Http
{
    public class CreateModelRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public IFormFile[] Files { get; set; } = Array.Empty<IFormFile>();
        
        public string? ThumbnailFileId { get; set; }
        
        public string? Privacy { get; set; } = "public";
        
        public string? License { get; set; }
        
        public bool AIGenerated { get; set; }
        
        public bool WorkInProgress { get; set; }
        
        public bool NSFW { get; set; }
        
        public bool Remix { get; set; }
    }
} 