namespace PolyBucket.Tests;

public static class TestContainerImages
{
    private const string DefaultMinioImage = "minio/minio:RELEASE.2025-04-08T15-41-24Z";

    public static string MinioImage =>
        Environment.GetEnvironmentVariable("POLYBUCKET_TEST_MINIO_IMAGE") ?? DefaultMinioImage;
}
