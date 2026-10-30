using System.Text;
using Shouldly;

namespace PolyBucket.Storage.Tests;

[Collection("StorageTestCollection")]
public class MinioStorageServiceIntegrationTests
{
    [Fact(DisplayName = "When an object is uploaded, download and presigned URL return the same content.")]
    public async Task UploadDownloadAndPresign_RoundTrip()
    {
        // Arrange
        var storage = StorageTestCollectionFixture.CreateStorageService();
        var objectKey = $"tests/{Guid.NewGuid():N}.txt";
        var content = "polybucket storage integration test";
        await using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var returnedKey = await storage.UploadAsync(objectKey, uploadStream, "text/plain");
        await using var downloaded = await storage.DownloadAsync(returnedKey);
        using var reader = new StreamReader(downloaded, Encoding.UTF8, leaveOpen: true);
        var downloadedText = await reader.ReadToEndAsync();
        var presignedUrl = await storage.GetPresignedUrlAsync(returnedKey, TimeSpan.FromMinutes(5));

        // Assert
        returnedKey.ShouldBe(objectKey);
        downloadedText.ShouldBe(content);
        presignedUrl.ShouldNotBeNullOrWhiteSpace();
        presignedUrl.ShouldContain(objectKey);

        await storage.DeleteAsync(objectKey);
    }

    [Fact(DisplayName = "When provider is minio, upload and download still succeed against Silo.")]
    public async Task UploadDownload_Works_ForMinioProviderId()
    {
        // Arrange
        var storage = StorageTestCollectionFixture.CreateStorageService(StorageProviderIds.Minio);
        var objectKey = $"tests/{Guid.NewGuid():N}.txt";
        var content = "minio provider label";
        await using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        await storage.UploadAsync(objectKey, uploadStream, "text/plain");
        await using var downloaded = await storage.DownloadAsync(objectKey);
        using var reader = new StreamReader(downloaded, Encoding.UTF8, leaveOpen: true);
        var downloadedText = await reader.ReadToEndAsync();

        // Assert
        downloadedText.ShouldBe(content);
        await storage.DeleteAsync(objectKey);
    }
}
