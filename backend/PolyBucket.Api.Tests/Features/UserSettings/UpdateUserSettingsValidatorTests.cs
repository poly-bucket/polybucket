using PolyBucket.Api.Features.Printers;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.UserSettings;

public class UpdateUserSettingsValidatorTests
{
    [Fact]
    public void Validate_WhenLanguageIsValid_NormalizesAndPasses()
    {
        var command = new UpdateUserSettingsCommand { Language = "EN" };

        UpdateUserSettingsValidator.Validate(command);

        command.Language.ShouldBe("en");
    }

    [Fact]
    public void Validate_WhenLanguageIsInvalid_Throws()
    {
        var command = new UpdateUserSettingsCommand { Language = "fr" };

        Should.Throw<UpdateUserSettingsValidationException>(() => UpdateUserSettingsValidator.Validate(command));
    }

    [Fact]
    public void Validate_WhenPrinterIdIsUnknown_Throws()
    {
        var command = new UpdateUserSettingsCommand
        {
            ApplyDefaultPrinterId = true,
            DefaultPrinterId = Guid.NewGuid()
        };

        Should.Throw<UpdateUserSettingsValidationException>(() => UpdateUserSettingsValidator.Validate(command));
    }

    [Fact]
    public void Validate_WhenPrinterIdIsCatalogEntry_Passes()
    {
        var command = new UpdateUserSettingsCommand { DefaultPrinterId = PrinterCatalog.PrusaMk3sPlusId };

        Should.NotThrow(() => UpdateUserSettingsValidator.Validate(command));
    }
}