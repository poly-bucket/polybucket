using PolyBucket.Api.Common.Entities;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.Domain
{
    public class FileTypeSettings : Auditable
    {
        [Required]
        [StringLength(20)]
        public string FileExtension { get; set; } = string.Empty;
        
        public bool Enabled { get; set; } = true;
        
        [Range(1, long.MaxValue)]
        public long MaxFileSizeBytes { get; set; } = 100L * 1024 * 1024; // 100MB default
        
        [Range(1, 100)]
        public int MaxPerUpload { get; set; } = 5;
        
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string MimeType { get; set; } = string.Empty;
        
        public bool RequiresPreview { get; set; } = false;
        
        public bool IsCompressible { get; set; } = true;
        
        [StringLength(256)]
        public string Category { get; set; } = string.Empty; // 3D, Image, Document, Archive
        
        public int Priority { get; set; } = 0; // For ordering in UI
        
        public bool IsDefault { get; set; } = false; // Whether this is a default file type
    }
}
