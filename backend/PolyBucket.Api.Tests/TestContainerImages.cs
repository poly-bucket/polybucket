namespace PolyBucket.Api.Tests;

public static class TestContainerImages
{
    private const string DefaultMinioImage = "pgsty/silo@sha256:7bb2aff6b89efe1cb1303c10fc21d579447434ed06649ac9ec05a5e2dbf30ad7";

    public static string MinioImage =>
        Environment.GetEnvironmentVariable("POLYBUCKET_TEST_MINIO_IMAGE") ?? DefaultMinioImage;
}
