using PolyBucket.Api.Features.Files.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Files.Domain;

public class ModelFileNameHelperTests
{
    [Theory(DisplayName = "GetLeafFileName strips directory prefixes.")]
    [InlineData("images/photo.jpg", "photo.jpg")]
    [InlineData("files/switch_shelf.stl", "switch_shelf.stl")]
    [InlineData(@"images\photo.jpg", "photo.jpg")]
    [InlineData("README.txt", "README.txt")]
    [InlineData("photo.jpg", "photo.jpg")]
    public void GetLeafFileName_StripsPathPrefixes(string input, string expected)
    {
        var result = ModelFileNameHelper.GetLeafFileName(input);

        result.ShouldBe(expected);
    }

    [Fact(DisplayName = "GetLeafFileName returns empty for null or whitespace.")]
    public void GetLeafFileName_ReturnsEmptyForBlankInput()
    {
        ModelFileNameHelper.GetLeafFileName(null).ShouldBe(string.Empty);
        ModelFileNameHelper.GetLeafFileName("   ").ShouldBe(string.Empty);
    }
}