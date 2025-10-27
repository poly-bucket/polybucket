using System.ComponentModel.DataAnnotations;
using MediatR;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Domain
{
    public class UpdateSiteModelSettingsCommand : IRequest<UpdateSiteModelSettingsResponse>
    {
        // File Upload Settings
        [Range(1 * 1024 * 1024, 1024 * 1024 * 1024, ErrorMessage = "Max file size must be between 1MB and 1GB")]
        public long MaxFileSizeBytes { get; set; } = 100L * 1024 * 1024; // 100MB
        
        [Required(ErrorMessage = "Allowed file types are required")]
        [StringLength(1000, ErrorMessage = "Allowed file types cannot exceed 1000 characters")]
        public string AllowedFileTypes { get; set; } = ".stl,.obj,.fbx,.3ds,.glb,.gltf,.ply,.step,.stp,.iges,.igs,.brep,.png,.jpg,.jpeg,.gif";
        
        [Range(1, 20, ErrorMessage = "Max files per upload must be between 1 and 20")]
        public int MaxFilesPerUpload { get; set; } = 5;
        
        public bool EnableFileCompression { get; set; } = true;
        public bool AutoGeneratePreviews { get; set; } = true;
        
        // Default Model Settings
        [Required(ErrorMessage = "Default model privacy is required")]
        public string DefaultModelPrivacy { get; set; } = "Public";
        
        public bool AutoApproveModels { get; set; } = false;
        public bool RequireModeration { get; set; } = true;
        
        // Upload Behavior
        public bool RequireLoginForUpload { get; set; } = true;
        public bool AllowPublicBrowsing { get; set; } = true;

        public bool IsValid(out List<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            var context = new ValidationContext(this);
            
            // Validate DefaultModelPrivacy enum value
            if (!Enum.TryParse<PrivacySettings>(DefaultModelPrivacy, true, out _))
            {
                validationResults.Add(new ValidationResult("Invalid default model privacy value. Must be one of: Public, Private, Unlisted", new[] { nameof(DefaultModelPrivacy) }));
            }
            
            return validationResults.Count == 0;
        }
    }

    public class UpdateSiteModelSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
