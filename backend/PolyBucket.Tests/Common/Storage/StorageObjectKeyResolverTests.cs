using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Settings;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Common.Storage;

public class StorageObjectKeyResolverTests
{
    private readonly StorageObjectKeyResolver _resolver = new(
        Options.Create(new StorageSettings { BucketName = "polybucket-uploads" }),
        NullLogger<StorageObjectKeyResolver>.Instance);

    [Fact(DisplayName = "When the stored value is a plain object key, Resolve returns it unchanged.")]
    public void Resolve_PlainKey_ReturnsKey()
    {
        // Arrange
        const string key = "models/abc/file.stl";

        // Act
        var result = _resolver.Resolve(key);

        // Assert
        result.ShouldBe(key);
    }

    [Fact(DisplayName = "When the stored value is a path-style presigned URL, Resolve returns the object key.")]
    public void Resolve_PathStyleUrl_ReturnsObjectKey()
    {
        // Arrange
        const string url = "http://localhost:8333/polybucket-uploads/models/abc/file.stl?X-Amz-Signature=abc";

        // Act
        var result = _resolver.Resolve(url);

        // Assert
        result.ShouldBe("models/abc/file.stl");
    }

    [Fact(DisplayName = "When the bucket name is missing from the URL path, Resolve returns null.")]
    public void Resolve_UrlWithoutBucket_ReturnsNull()
    {
        // Arrange
        const string url = "http://localhost:8333/other-bucket/file.stl";

        // Act
        var result = _resolver.Resolve(url);

        // Assert
        result.ShouldBeNull();
    }

    [Fact(DisplayName = "When the stored value is null or empty, Resolve returns null.")]
    public void Resolve_NullOrEmpty_ReturnsNull()
    {
        // Act & Assert
        _resolver.Resolve(null).ShouldBeNull();
        _resolver.Resolve("").ShouldBeNull();
    }
}
