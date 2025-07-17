using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.Domain;

public class TestEmailConfigurationCommand
{
    [Required(ErrorMessage = "Test email address is required")]
    [EmailAddress(ErrorMessage = "Test email address must be a valid email address")]
    public string TestEmailAddress { get; set; } = string.Empty;
    
    public EmailSettings? EmailSettings { get; set; }
} 