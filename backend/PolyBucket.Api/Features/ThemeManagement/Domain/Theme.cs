using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.ThemeManagement.Domain;

public class Theme
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public bool IsDefault { get; set; } = false;
    
    [Required]
    public bool IsActive { get; set; } = false;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual ThemeColors Colors { get; set; } = null!;
}

public class ThemeColors
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ThemeId { get; set; }
    
    // Primary Colors
    [Required]
    [StringLength(7)]
    public string Primary { get; set; } = null!;
    
    [Required]
    [StringLength(7)]
    public string PrimaryLight { get; set; } = null!;
    
    [Required]
    [StringLength(7)]
    public string PrimaryDark { get; set; } = null!;
    
    // Secondary Colors
    [Required]
    [StringLength(7)]
    public string Secondary { get; set; } = null!;
    
    [Required]
    [StringLength(7)]
    public string SecondaryLight { get; set; } = null!;
    
    [Required]
    [StringLength(7)]
    public string SecondaryDark { get; set; } = null!;
    
    // Accent Colors
    [Required]
    [StringLength(7)]
    public string Accent { get; set; } = null!;
    
    [Required]
    [StringLength(7)]
    public string AccentLight { get; set; } = null!;
    
    [Required]
    [StringLength(7)]
    public string AccentDark { get; set; } = null!;
    
    // Background Colors
    [Required]
    [StringLength(7)]
    public string BackgroundPrimary { get; set; } = null!;
    
    [Required]
    [StringLength(7)]
    public string BackgroundSecondary { get; set; } = null!;
    
    [Required]
    [StringLength(7)]
    public string BackgroundTertiary { get; set; } = null!;
    
    // Navigation property back to Theme
    public virtual Theme Theme { get; set; } = null!;
}
