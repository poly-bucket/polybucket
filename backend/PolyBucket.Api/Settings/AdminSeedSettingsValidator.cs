using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace PolyBucket.Api.Settings;

public sealed class AdminSeedSettingsValidator(IHostEnvironment hostEnvironment) : IValidateOptions<AdminSeedSettings>
{
    public ValidateOptionsResult Validate(string? name, AdminSeedSettings options)
    {
        if (hostEnvironment.IsDevelopment())
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            return ValidateOptionsResult.Fail(
                "Admin:Password (or environment variable Admin__Password) is required in non-development environments for the bootstrap admin account.");
        }

        return ValidateOptionsResult.Success;
    }
}
