using System.ComponentModel.DataAnnotations;
using MediatR;
using System.Collections.Generic;
using System.ComponentModel;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Domain
{
    public class UpdateSiteSettingsCommand : IRequest<UpdateSiteSettingsResponse>
    {
        [Required(ErrorMessage = "Site name is required")]
        [StringLength(100, ErrorMessage = "Site name cannot exceed 100 characters")]
        public string SiteName { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Site description cannot exceed 500 characters")]
        public string SiteDescription { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Contact email is required")]
        [EmailAddress(ErrorMessage = "Contact email must be a valid email address")]
        public string ContactEmail { get; set; } = string.Empty;
        
        public bool AllowPublicBrowsing { get; set; } = true;
        public bool RequireLoginForUpload { get; set; } = true;
        public bool AllowUserRegistration { get; set; } = true;
        public bool RequireEmailVerification { get; set; } = false;
        
        // File Upload Settings
        public long MaxFileSizeBytes { get; set; } = 100L * 1024 * 1024; // 100MB
        
        [Required(ErrorMessage = "Allowed file types are required")]
        public string AllowedFileTypes { get; set; } = ".stl,.obj,.fbx,.3ds,.glb,.gltf,.ply,.step,.stp,.iges,.igs,.brep,.png,.jpg,.jpeg,.gif";
        
        [Range(1, 20, ErrorMessage = "Max files per upload must be between 1 and 20")]
        public int MaxFilesPerUpload { get; set; } = 5;
        
        public bool EnableFileCompression { get; set; } = true;
        public bool AutoGeneratePreviews { get; set; } = true;
        
        // Default Model Settings
        public PrivacySettings DefaultModelPrivacy { get; set; } = PrivacySettings.Public;
        public bool AutoApproveModels { get; set; } = false;
        public bool RequireModeration { get; set; } = true;
        
        // Role Settings
        public string DefaultUserRole { get; set; } = "User";
        public bool AllowCustomRoles { get; set; } = false;
        
        // Security Settings
        [Range(1, 10, ErrorMessage = "Max failed login attempts must be between 1 and 10")]
        public int MaxFailedLoginAttempts { get; set; } = 5;
        
        [Range(5, 60, ErrorMessage = "Lockout duration must be between 5 and 60 minutes")]
        public int LockoutDurationMinutes { get; set; } = 15;
        
        public bool RequireStrongPasswords { get; set; } = true;
        
        [Range(6, 20, ErrorMessage = "Password minimum length must be between 6 and 20 characters")]
        public int PasswordMinLength { get; set; } = 8;
        
        // UI Settings
        public string DefaultTheme { get; set; } = "dark";
        public string DefaultLanguage { get; set; } = "en";
        public bool ShowAdvancedOptions { get; set; } = false;
        
        // Federation Settings
        public bool EnableFederation { get; set; } = false;
        
        [StringLength(100, ErrorMessage = "Instance name cannot exceed 100 characters")]
        public string InstanceName { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Instance description cannot exceed 500 characters")]
        public string InstanceDescription { get; set; } = string.Empty;
        
        public string? AdminContact { get; set; }

        public bool IsValid(out List<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            var context = new ValidationContext(this);
            
            // Validate MaxFileSizeBytes
            var minSize = 1L * 1024 * 1024; // 1MB in bytes
            var maxSize = 1024L * 1024 * 1024; // 1024MB in bytes
            if (MaxFileSizeBytes < minSize || MaxFileSizeBytes > maxSize)
            {
                validationResults.Add(new ValidationResult("Max file size must be between 1 and 1024 MB", new[] { nameof(MaxFileSizeBytes) }));
            }
            
            // Validate AdminContact if provided
            if (!string.IsNullOrEmpty(AdminContact))
            {
                var emailAttribute = new EmailAddressAttribute();
                if (!emailAttribute.IsValid(AdminContact))
                {
                    validationResults.Add(new ValidationResult("Admin contact must be a valid email address", new[] { nameof(AdminContact) }));
                }
            }
            
            return validationResults.Count == 0;
        }
    }

    public class UpdateSiteSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NextStep { get; set; } = string.Empty;
    }
} 