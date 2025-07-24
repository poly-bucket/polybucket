using System.ComponentModel.DataAnnotations;
using MediatR;
using System.Collections.Generic;
using System.ComponentModel;

namespace PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain
{
    public class UpdateThemeSettingsCommand : IRequest<UpdateThemeSettingsResponse>
    {
        [Required(ErrorMessage = "Primary color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary color must be a valid hex color")]
        public string PrimaryColor { get; set; } = "#6366f1";
        
        [Required(ErrorMessage = "Primary light color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary light color must be a valid hex color")]
        public string PrimaryLightColor { get; set; } = "#818cf8";
        
        [Required(ErrorMessage = "Primary dark color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary dark color must be a valid hex color")]
        public string PrimaryDarkColor { get; set; } = "#4f46e5";
        
        [Required(ErrorMessage = "Secondary color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Secondary color must be a valid hex color")]
        public string SecondaryColor { get; set; } = "#8b5cf6";
        
        [Required(ErrorMessage = "Secondary light color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Secondary light color must be a valid hex color")]
        public string SecondaryLightColor { get; set; } = "#a78bfa";
        
        [Required(ErrorMessage = "Secondary dark color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Secondary dark color must be a valid hex color")]
        public string SecondaryDarkColor { get; set; } = "#7c3aed";
        
        [Required(ErrorMessage = "Accent color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Accent color must be a valid hex color")]
        public string AccentColor { get; set; } = "#06b6d4";
        
        [Required(ErrorMessage = "Accent light color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Accent light color must be a valid hex color")]
        public string AccentLightColor { get; set; } = "#22d3ee";
        
        [Required(ErrorMessage = "Accent dark color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Accent dark color must be a valid hex color")]
        public string AccentDarkColor { get; set; } = "#0891b2";
        
        [Required(ErrorMessage = "Background primary color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Background primary color must be a valid hex color")]
        public string BackgroundPrimaryColor { get; set; } = "#0f0f23";
        
        [Required(ErrorMessage = "Background secondary color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Background secondary color must be a valid hex color")]
        public string BackgroundSecondaryColor { get; set; } = "#1a1a2e";
        
        [Required(ErrorMessage = "Background tertiary color is required")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Background tertiary color must be a valid hex color")]
        public string BackgroundTertiaryColor { get; set; } = "#16213e";

        public bool IsValid(out List<ValidationResult> validationResults)
        {
            var context = new ValidationContext(this);
            validationResults = new List<ValidationResult>();
            return Validator.TryValidateObject(this, context, validationResults, true);
        }
    }

    public class UpdateThemeSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsThemeCustomized { get; set; }
    }
} 