using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Database.Entities.Models.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.Common;

public class ModelPrivacyResolverTests
{
    [Theory(DisplayName = "ModelPrivacyResolver maps request strings to privacy settings.")]
    [InlineData("private", PrivacySettings.Private)]
    [InlineData("PUBLIC", PrivacySettings.Public)]
    [InlineData("unlisted", PrivacySettings.Unlisted)]
  public void Resolve_MapsRequest(string request, PrivacySettings expected)
    {
        // Act
        var privacy = ModelPrivacyResolver.Resolve(request, PrivacySettings.Public);

        // Assert
        privacy.ShouldBe(expected);
    }

    [Fact(DisplayName = "When the request privacy is omitted, ModelPrivacyResolver uses the server default.")]
    public void Resolve_UsesServerDefaultWhenOmitted()
    {
        // Act
        var privacy = ModelPrivacyResolver.Resolve("  ", PrivacySettings.Private);

        // Assert
        privacy.ShouldBe(PrivacySettings.Private);
    }
}