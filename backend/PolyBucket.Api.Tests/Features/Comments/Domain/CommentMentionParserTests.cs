using PolyBucket.Api.Features.Comments.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.Domain;

public class CommentMentionParserTests
{
    [Fact(DisplayName = "ExtractUsernames returns distinct valid mentions.")]
    public void ExtractUsernames_ReturnsDistinctMentions()
    {
        // Arrange
        const string content = "Hi @alice and @bob — @alice again";

        // Act
        var names = CommentMentionParser.ExtractUsernames(content);

        // Assert
        names.Count.ShouldBe(2);
        names.ShouldContain("alice");
        names.ShouldContain("bob");
    }

    [Fact(DisplayName = "ExtractUsernames ignores tokens that are too short.")]
    public void ExtractUsernames_IgnoresShortTokens()
    {
        // Arrange
        const string content = "@ab should not match";

        // Act
        var names = CommentMentionParser.ExtractUsernames(content);

        // Assert
        names.ShouldBeEmpty();
    }

    [Fact(DisplayName = "ExtractUsernames does not treat email addresses as mentions.")]
    public void ExtractUsernames_IgnoresEmailLocalPart()
    {
        // Arrange
        const string content = "Contact me at user@example.com";

        // Act
        var names = CommentMentionParser.ExtractUsernames(content);

        // Assert
        names.ShouldBeEmpty();
    }
}