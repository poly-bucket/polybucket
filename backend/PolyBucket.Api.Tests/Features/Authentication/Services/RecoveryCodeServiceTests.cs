using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.Services;

public class RecoveryCodeServiceTests
{
    private readonly IRecoveryCodeService _service = new RecoveryCodeService(
        Options.Create(new RecoveryCodeSettings { CodeCount = 3, CodeLength = 12 }),
        new RecoveryCodeProtector(new EphemeralDataProtectionProvider()));

    [Fact(DisplayName = "When creating a batch, the requested number of unique codes is returned.")]
    public void CreateBatchEntities_ReturnsConfiguredCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var result = _service.CreateBatchEntities(userId, actorId, now);

        // Assert
        result.PlaintextCodes.Count.ShouldBe(3);
        result.Entities.Count.ShouldBe(3);
        result.PlaintextCodes.Distinct(StringComparer.OrdinalIgnoreCase).Count().ShouldBe(3);
        result.Entities.All(e => e.UserId == userId && !e.IsUsed).ShouldBeTrue();
    }

    [Fact(DisplayName = "When matching a valid code, the corresponding entity is returned.")]
    public void TryMatchCode_WithValidCode_ReturnsEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var batch = _service.CreateBatchEntities(userId, userId, DateTime.UtcNow);
        var code = batch.PlaintextCodes[0];

        // Act
        var matched = _service.TryMatchCode(batch.Entities, code, out var entity);

        // Assert
        matched.ShouldBeTrue();
        entity.ShouldNotBeNull();
    }

    [Fact(DisplayName = "When matching an invalid code, no entity is returned.")]
    public void TryMatchCode_WithInvalidCode_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var batch = _service.CreateBatchEntities(userId, userId, DateTime.UtcNow);

        // Act
        var matched = _service.TryMatchCode(batch.Entities, "INVALIDCODE1", out var entity);

        // Assert
        matched.ShouldBeFalse();
        entity.ShouldBeNull();
    }

    [Fact(DisplayName = "When normalizing input with dashes, matching still succeeds.")]
    public void TryMatchCode_WithDashes_NormalizesAndMatches()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var batch = _service.CreateBatchEntities(userId, userId, DateTime.UtcNow);
        var raw = batch.PlaintextCodes[1];
        var formatted = $"{raw[..4]}-{raw[4..8]}-{raw[8..]}";

        // Act
        var matched = _service.TryMatchCode(batch.Entities, formatted, out _);

        // Assert
        matched.ShouldBeTrue();
    }

    [Fact(DisplayName = "When serializing display payload, codes can be deserialized.")]
    public void SerializeDisplayPayload_RoundTrip_PreservesCodes()
    {
        // Arrange
        var codes = new[] { "AAAA1111BBBB", "CCCC2222DDDD" };

        // Act
        var protectedPayload = _service.SerializeDisplayPayload(codes);
        var restored = _service.DeserializeDisplayPayload(protectedPayload);

        // Assert
        restored.ShouldNotBeNull();
        restored!.ShouldBe(codes);
    }
}
