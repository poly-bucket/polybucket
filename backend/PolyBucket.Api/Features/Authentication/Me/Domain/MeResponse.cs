namespace PolyBucket.Api.Features.Authentication.Me.Domain;

public class MeResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string? PendingEmail { get; set; }
    public bool EmailVerificationRequired { get; set; }
    public bool EmailDeliveryAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public bool HasCompletedFirstTimeSetup { get; set; }
    public string? Avatar { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public MeUserSettingsResponse? Settings { get; set; }
}

public class MeUserSettingsResponse
{
    public string Language { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; }
    public string MeasurementSystem { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}
