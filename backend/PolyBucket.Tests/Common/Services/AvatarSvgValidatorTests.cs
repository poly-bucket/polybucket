using PolyBucket.Api.Common.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Common.Services;

public class AvatarSvgValidatorTests
{
    private readonly AvatarSvgValidator _validator = new();

    [Fact(DisplayName = "When SVG is valid, validation succeeds.")]
    public void ValidSvg_Succeeds()
    {
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\"><rect/></svg>";
        var ok = _validator.TryValidate(svg, out var sanitized, out var error);
        ok.ShouldBeTrue();
        error.ShouldBeNull();
        sanitized.ShouldNotBeNull();
    }

    [Fact(DisplayName = "When avatar is a data URL, validation fails.")]
    public void DataUrl_Fails()
    {
        var ok = _validator.TryValidate("data:image/png;base64,abc", out _, out var error);
        ok.ShouldBeFalse();
        error.ShouldNotBeNullOrEmpty();
    }

    [Fact(DisplayName = "When SVG contains script, script is stripped.")]
    public void Script_IsStripped()
    {
        var svg = "<svg><script>alert(1)</script><circle/></svg>";
        var ok = _validator.TryValidate(svg, out var sanitized, out _);
        ok.ShouldBeTrue();
        sanitized!.ShouldNotContain("script");
    }
}
