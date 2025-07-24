using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.Domain;

public class AuthenticationSettings
{
    public LoginMethod LoginMethod { get; set; } = LoginMethod.Email;
    
    public bool AllowEmailLogin { get; set; } = true;
    
    public bool AllowUsernameLogin { get; set; } = false;
    
    public bool RequireEmailVerification { get; set; } = false;
    
    public int MaxFailedLoginAttempts { get; set; } = 5;
    
    public int LockoutDurationMinutes { get; set; } = 15;
    
    public bool RequireStrongPasswords { get; set; } = true;
    
    public int PasswordMinLength { get; set; } = 8;
    
    public bool IsValid()
    {
        // At least one login method must be enabled
        if (!AllowEmailLogin && !AllowUsernameLogin)
        {
            return false;
        }
        
        // Login method must match the allowed methods
        if (LoginMethod == LoginMethod.Email && !AllowEmailLogin)
        {
            return false;
        }
        
        if (LoginMethod == LoginMethod.Username && !AllowUsernameLogin)
        {
            return false;
        }
        
        if (LoginMethod == LoginMethod.Both && (!AllowEmailLogin || !AllowUsernameLogin))
        {
            return false;
        }
        
        return MaxFailedLoginAttempts > 0 && 
               LockoutDurationMinutes > 0 && 
               PasswordMinLength >= 6;
    }
}

public enum LoginMethod
{
    Email,
    Username,
    Both
} 