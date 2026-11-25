using System.IO.Compression;
using System.Text;
using PolyBucket.Api.Features.Models.Common;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.Common;

public class ModelStorageContentHelperTests
{
    [Fact]
    public void IsCompressedStoragePath_WhenKeyEndsWithGz_ReturnsTrue()
    {
        ModelStorageContentHelper.IsCompressedStoragePath("models/x/file.stl.gz").ShouldBeTrue();
    }

    [Fact]
    public async Task OpenStoredContentAsync_WhenCompressed_ReturnsDecompressedBytes()
    {
        const string original = "solid test";
        await using var raw = new MemoryStream();
        await using (var gzip = new GZipStream(raw, CompressionLevel.Optimal, leaveOpen: true))
        await using (var writer = new StreamWriter(gzip, Encoding.UTF8, leaveOpen: true))
        {
            await writer.WriteAsync(original);
        }

        raw.Position = 0;
        await using var opened = await ModelStorageContentHelper.OpenStoredContentAsync(
            raw,
            "models/1/part.stl.gz");
        using var reader = new StreamReader(opened, Encoding.UTF8);
        var text = await reader.ReadToEndAsync();

        text.ShouldBe(original);
    }
}