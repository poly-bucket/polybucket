using PolyBucket.Api.Features.Authentication.Me.Repository;
using PolyBucket.Email;

namespace PolyBucket.Api.Features.Authentication.Me.Domain;

public class MeService(IMeRepository repository, IEmailSettingsResolver emailSettingsResolver) : IMeService
{
    private readonly IMeRepository _repository = repository;
    private readonly IEmailSettingsResolver _emailSettingsResolver = emailSettingsResolver;

    public async Task<MeResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _repository.GetCurrentUserAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }

        var emailSettings = await _emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);

        return new MeResponse
        {
            EmailDeliveryAvailable = emailSettings.CanDeliver,
            EmailVerificationRequired = emailSettings.CanDeliver && emailSettings.RequireEmailVerification,
            Id = user.Id.ToString(),
            Username = user.Username ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role?.Name ?? "User",
            IsEmailVerified = user.EmailVerifiedAt.HasValue,
            EmailVerifiedAt = user.EmailVerifiedAt,
            PendingEmail = user.PendingEmail,
            CreatedAt = user.CreatedAt,
            RequiresPasswordChange = user.RequiresPasswordChange,
            HasCompletedFirstTimeSetup = user.HasCompletedFirstTimeSetup,
            Avatar = user.Avatar,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Settings = user.Settings != null ? new MeUserSettingsResponse
            {
                Language = user.Settings.Language,
                Theme = user.Settings.Theme,
                EmailNotifications = user.Settings.EmailNotifications,
                MeasurementSystem = user.Settings.MeasurementSystem,
                TimeZone = user.Settings.TimeZone
            } : null
        };
    }
}
