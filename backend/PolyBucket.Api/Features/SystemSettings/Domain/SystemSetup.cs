using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System;

namespace PolyBucket.Api.Features.SystemSettings.Domain
{
    public class SystemSetup : Auditable
    {
        public bool IsFirstTimeSetup { get; set; } = true;
        public bool IsAdminConfigured { get; set; } = false;
        public bool IsSiteConfigured { get; set; } = false;
        public bool IsEmailConfigured { get; set; } = false;
        public bool IsModerationConfigured { get; set; } = false;
        
        // Site Configuration
        public string SiteName { get; set; } = "PolyBucket";
        public string SiteDescription { get; set; } = "3D Model Repository";
        public string ContactEmail { get; set; } = string.Empty;
        public bool AllowPublicBrowsing { get; set; } = true;
        public bool RequireLoginForUpload { get; set; } = true;
        public bool AllowUserRegistration { get; set; } = true;
        public bool RequireEmailVerification { get; set; } = false;
        
        // File Upload Settings
        public long MaxFileSizeBytes { get; set; } = 100L * 1024 * 1024; // 100MB
        public string AllowedFileTypes { get; set; } = ".stl,.obj,.fbx,.3ds,.glb,.gltf,.ply,.step,.stp,.iges,.igs,.brep,.png,.jpg,.jpeg,.gif";
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
        public int MaxFailedLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public bool RequireStrongPasswords { get; set; } = true;
        public int PasswordMinLength { get; set; } = 8;
        
        // UI Settings
        public string DefaultTheme { get; set; } = "dark";
        public string DefaultLanguage { get; set; } = "en";
        public bool ShowAdvancedOptions { get; set; } = false;
        
        // Theme Customization Settings
        public string PrimaryColor { get; set; } = "#6366f1";
        public string PrimaryLightColor { get; set; } = "#818cf8";
        public string PrimaryDarkColor { get; set; } = "#4f46e5";
        public string SecondaryColor { get; set; } = "#8b5cf6";
        public string SecondaryLightColor { get; set; } = "#a78bfa";
        public string SecondaryDarkColor { get; set; } = "#7c3aed";
        public string AccentColor { get; set; } = "#06b6d4";
        public string AccentLightColor { get; set; } = "#22d3ee";
        public string AccentDarkColor { get; set; } = "#0891b2";
        public string BackgroundPrimaryColor { get; set; } = "#0f0f23";
        public string BackgroundSecondaryColor { get; set; } = "#1a1a2e";
        public string BackgroundTertiaryColor { get; set; } = "#16213e";
        public bool IsThemeCustomized { get; set; } = false;
        
        // Extensible Theme System
        public string? ActiveThemeId { get; set; } = "liquid-glass-default";
        public string? ThemeConfiguration { get; set; }
        
        // Federation Settings
        public bool EnableFederation { get; set; } = false;
        public string InstanceName { get; set; } = string.Empty;
        public string InstanceDescription { get; set; } = string.Empty;
        public string AdminContact { get; set; } = string.Empty;
        
        // Setup Completion Tracking
        public DateTime? SetupCompletedAt { get; set; }
        public Guid? SetupCompletedByUserId { get; set; }
    }
} 