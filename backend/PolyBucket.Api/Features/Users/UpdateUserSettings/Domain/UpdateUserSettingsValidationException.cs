namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;

public sealed class UpdateUserSettingsValidationException(string message) : Exception(message);