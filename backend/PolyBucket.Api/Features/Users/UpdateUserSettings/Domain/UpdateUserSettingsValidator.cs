using System;
using PolyBucket.Api.Features.Printers;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;

public static class UpdateUserSettingsValidator
{
    private static readonly HashSet<string> AllowedLanguages = new(StringComparer.OrdinalIgnoreCase) { "en" };
    private static readonly HashSet<string> AllowedThemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "light",
        "dark",
        "system"
    };
    private static readonly HashSet<string> AllowedMeasurementSystems = new(StringComparer.OrdinalIgnoreCase)
    {
        "metric",
        "imperial"
    };

    public static void Validate(UpdateUserSettingsCommand command)
    {
        if (command.Language != null)
        {
            var language = command.Language.Trim();
            if (string.IsNullOrEmpty(language) || !AllowedLanguages.Contains(language))
            {
                throw new UpdateUserSettingsValidationException("Invalid language.");
            }

            command.Language = language.ToLowerInvariant();
        }

        if (command.Theme != null)
        {
            var theme = command.Theme.Trim();
            if (!AllowedThemes.Contains(theme))
            {
                throw new UpdateUserSettingsValidationException("Invalid theme.");
            }

            command.Theme = theme.ToLowerInvariant();
        }

        if (command.MeasurementSystem != null)
        {
            var measurement = command.MeasurementSystem.Trim();
            if (!AllowedMeasurementSystems.Contains(measurement))
            {
                throw new UpdateUserSettingsValidationException("Invalid measurement system.");
            }

            command.MeasurementSystem = measurement.ToLowerInvariant();
        }

        if (command.TimeZone != null)
        {
            var timeZone = command.TimeZone.Trim();
            if (string.IsNullOrEmpty(timeZone))
            {
                throw new UpdateUserSettingsValidationException("Invalid time zone.");
            }

            try
            {
                _ = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new UpdateUserSettingsValidationException("Invalid time zone.");
            }
            catch (InvalidTimeZoneException)
            {
                throw new UpdateUserSettingsValidationException("Invalid time zone.");
            }

            command.TimeZone = timeZone;
        }

        if (command.ApplyDefaultPrinterId &&
            command.DefaultPrinterId.HasValue &&
            !PrinterCatalog.Contains(command.DefaultPrinterId.Value))
        {
            throw new UpdateUserSettingsValidationException("Invalid default printer.");
        }
    }
}