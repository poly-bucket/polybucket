using System;
using PolyBucket.Api.Features.Comments.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments;

public class CommentTargetParserTests
{
    [Theory(DisplayName = "When the target type is a known alias, it parses to the matching target type.")]
    [InlineData("model", CommentTargetType.Model)]
    [InlineData("MODEL", CommentTargetType.Model)]
    [InlineData("user", CommentTargetType.UserProfile)]
    [InlineData("userprofile", CommentTargetType.UserProfile)]
    [InlineData("collection", CommentTargetType.Collection)]
    [InlineData("report", CommentTargetType.Report)]
    public void TryParse_KnownType_Succeeds(string input, CommentTargetType expected)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var parsed = CommentTargetParser.TryParse(input, id, out var target);

        // Assert
        parsed.ShouldBeTrue();
        target.TargetType.ShouldBe(expected);
        target.TargetId.ShouldBe(id);
    }

    [Fact(DisplayName = "When the target type is unknown, parsing fails instead of throwing.")]
    public void TryParse_UnknownType_Fails()
    {
        // Arrange
        var input = "printer";

        // Act
        var parsed = CommentTargetParser.TryParse(input, Guid.NewGuid(), out _);

        // Assert
        parsed.ShouldBeFalse();
    }
}
