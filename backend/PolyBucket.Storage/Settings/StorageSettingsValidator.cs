using Microsoft.Extensions.Options;
using PolyBucket.Storage.Settings;

namespace PolyBucket.Storage.Settings;

public sealed class StorageSettingsValidator : IValidateOptions<StorageSettings>
{
    public ValidateOptionsResult Validate(string? name, StorageSettings options)
    {
        if (!StorageSettingsValidation.IsEnvConfigurationAttempted(options))
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateCompleteProfile(options);
    }

    internal static ValidateOptionsResult ValidateCompleteProfile(StorageSettings options)
    {
        var errors = StorageSettingsValidation.GetProfileValidationErrors(options);
        if (errors.Count == 0)
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail(string.Join(" ", errors));
    }
}
