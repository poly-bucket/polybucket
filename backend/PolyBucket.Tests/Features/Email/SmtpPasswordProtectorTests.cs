using Microsoft.AspNetCore.DataProtection;
using PolyBucket.Api.Common.Email;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email;

public class SmtpPasswordProtectorTests
{
    [Fact(DisplayName = "When a password is protected, the stored value is prefixed, does not contain the plaintext, and round-trips.")]
    public void Protect_ThenUnprotect_RoundTrips()
    {
        // Arrange
        var protector = new SmtpPasswordProtector(new EphemeralDataProtectionProvider());

        // Act
        var stored = protector.Protect("s3cret!");
        var result = protector.Unprotect(stored);

        // Assert
        stored.ShouldStartWith(SmtpPasswordProtector.Prefix);
        stored.ShouldNotContain("s3cret!");
        result.Plaintext.ShouldBe("s3cret!");
        result.IsLegacyPlaintext.ShouldBeFalse();
        result.IsUnreadable.ShouldBeFalse();
    }

    [Fact(DisplayName = "When a stored value has no prefix, it is treated as legacy plaintext.")]
    public void Unprotect_LegacyPlaintext_IsFlagged()
    {
        // Arrange
        var protector = new SmtpPasswordProtector(new EphemeralDataProtectionProvider());

        // Act
        var result = protector.Unprotect("plain-password");

        // Assert
        result.Plaintext.ShouldBe("plain-password");
        result.IsLegacyPlaintext.ShouldBeTrue();
    }

    [Fact(DisplayName = "When a value was encrypted with different keys, it is reported as unreadable instead of throwing.")]
    public void Unprotect_WithDifferentKeys_IsUnreadable()
    {
        // Arrange
        var original = new SmtpPasswordProtector(new EphemeralDataProtectionProvider());
        var other = new SmtpPasswordProtector(new EphemeralDataProtectionProvider());
        var stored = original.Protect("s3cret!");

        // Act
        var result = other.Unprotect(stored);

        // Assert
        result.IsUnreadable.ShouldBeTrue();
        result.Plaintext.ShouldBeEmpty();
    }
}
