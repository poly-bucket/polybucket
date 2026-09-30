using PolyBucket.Api.Common.Email;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email;

public class EmailOptionsValidatorTests
{
    private readonly EmailOptionsValidator _validator = new();

    [Fact(DisplayName = "When the Email section is empty, validation succeeds.")]
    public void Validate_EmptyOptions_Succeeds()
    {
        // Arrange
        var options = new EmailOptions();

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the port, From address, and public URL are invalid, validation fails with a message for each.")]
    public void Validate_InvalidValues_Fails()
    {
        // Arrange
        var options = new EmailOptions
        {
            FromAddress = "not-an-email",
            PublicBaseUrl = "models.example.com",
            Smtp = new SmtpOptions { Port = 70000 }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("Port");
        result.FailureMessage.ShouldContain("FromAddress");
        result.FailureMessage.ShouldContain("PublicBaseUrl");
    }

    [Fact(DisplayName = "When both Password and PasswordFile are set, validation fails.")]
    public void Validate_PasswordAndPasswordFile_Fails()
    {
        // Arrange
        var path = System.IO.Path.GetTempFileName();
        var options = new EmailOptions { Smtp = new SmtpOptions { Password = "a", PasswordFile = path } };

        try
        {
            // Act
            var result = _validator.Validate(null, options);

            // Assert
            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldContain("not both");
        }
        finally
        {
            System.IO.File.Delete(path);
        }
    }

    [Fact(DisplayName = "When the password file does not exist, validation fails at startup.")]
    public void Validate_MissingPasswordFile_Fails()
    {
        // Arrange
        var options = new EmailOptions { Smtp = new SmtpOptions { PasswordFile = "/does/not/exist/smtp-password" } };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("does not exist");
    }
}
