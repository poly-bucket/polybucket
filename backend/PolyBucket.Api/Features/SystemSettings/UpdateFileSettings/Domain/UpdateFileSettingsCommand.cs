using MediatR;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Domain
{
    public class UpdateFileSettingsCommand : IRequest<UpdateFileSettingsResponse>
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string FileExtension { get; set; } = string.Empty;
        
        public bool Enabled { get; set; }
        
        [Range(1, long.MaxValue)]
        public long MaxFileSizeBytes { get; set; }
        
        [Range(1, 100)]
        public int MaxPerUpload { get; set; }
        
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string MimeType { get; set; } = string.Empty;
        
        public bool RequiresPreview { get; set; }
        
        public bool IsCompressible { get; set; }
        
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        
        public int Priority { get; set; }
        
        public bool IsDefault { get; set; }

        public bool IsValid(out List<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            var context = new ValidationContext(this);
            return Validator.TryValidateObject(this, context, validationResults, true);
        }
    }

    public class UpdateFileSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
