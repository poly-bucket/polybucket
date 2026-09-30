using PolyBucket.Api.Features.Authentication.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Services;

public class TokenHasherTests
{
    [Fact(DisplayName = "When hashing a token, the result is a 64 character lowercase SHA-256 hex digest that differs from the input.")]
    public void Hash_ShouldReturnLowercaseSha256Hex()
    {
        // Arrange
        const string raw = "abc";

        // Act
        var hash = TokenHasher.Hash(raw);

        // Assert
        hash.ShouldBe("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
        hash.Length.ShouldBe(64);
        hash.ShouldNotBe(raw);
    }

    [Fact(DisplayName = "When hashing the same token twice, the result is identical so lookups by hash work.")]
    public void Hash_ShouldBeDeterministic()
    {
        // Arrange
        const string raw = "Zm9vYmFy-_token";

        // Act
        var first = TokenHasher.Hash(raw);
        var second = TokenHasher.Hash(raw);

        // Assert
        first.ShouldBe(second);
        TokenHasher.Hash(raw + "x").ShouldNotBe(first);
    }
}
