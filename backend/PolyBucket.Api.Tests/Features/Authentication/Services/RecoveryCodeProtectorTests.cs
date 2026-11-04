using Microsoft.AspNetCore.DataProtection;
using PolyBucket.Api.Features.Authentication.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.Services;

public class RecoveryCodeProtectorTests
{
    private readonly IRecoveryCodeProtector _protector = new RecoveryCodeProtector(
        new EphemeralDataProtectionProvider());

    [Fact(DisplayName = "When protecting and unprotecting a code hash, the original hash is recovered.")]
    public void ProtectHash_RoundTrip_ReturnsOriginal()
    {
        // Arrange
        var hash = TokenHasher.Hash("ABCD1234EFGH");

        // Act
        var protectedHash = _protector.ProtectHash(hash);
        var unprotected = _protector.UnprotectHash(protectedHash);

        // Assert
        protectedHash.ShouldStartWith(RecoveryCodeProtector.Prefix);
        unprotected.ShouldBe(hash);
    }

    [Fact(DisplayName = "When protecting and unprotecting a display payload, the original JSON is recovered.")]
    public void ProtectDisplayPayload_RoundTrip_ReturnsOriginal()
    {
        // Arrange
        const string json = "[\"CODE1\",\"CODE2\"]";

        // Act
        var protectedPayload = _protector.ProtectDisplayPayload(json);
        var unprotected = _protector.UnprotectDisplayPayload(protectedPayload);

        // Assert
        unprotected.ShouldBe(json);
    }
}
