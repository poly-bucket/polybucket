using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.Domain;

public class EmailSettings
{
    public bool Enabled { get; set; } = false;
    
    [Required(ErrorMessage = "SMTP Server is required when email is enabled")]
    public string SmtpServer { get; set; } = string.Empty;
    
    [Range(1, 65535, ErrorMessage = "SMTP Port must be between 1 and 65535")]
    public int SmtpPort { get; set; } = 587;
    
    public string SmtpUsername { get; set; } = string.Empty;
    
    public string SmtpPassword { get; set; } = string.Empty;
    
    public bool UseSsl { get; set; } = true;
    
    [Required(ErrorMessage = "From Address is required when email is enabled")]
    [EmailAddress(ErrorMessage = "From Address must be a valid email address")]
    public string FromAddress { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "From Name is required when email is enabled")]
    public string FromName { get; set; } = string.Empty;
    
    public bool RequireEmailVerification { get; set; } = true;

    public bool IsValid()
    {
        if (!Enabled) return true;
        
        return !string.IsNullOrWhiteSpace(SmtpServer) &&
               SmtpPort > 0 && SmtpPort <= 65535 &&
               !string.IsNullOrWhiteSpace(FromAddress) &&
               !string.IsNullOrWhiteSpace(FromName);
    }
} 