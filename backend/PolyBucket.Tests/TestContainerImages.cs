namespace PolyBucket.Tests;

public static class TestContainerImages
{
    private const string DefaultSeaweedfsImage = "chrislusf/seaweedfs:3.79";

    public static string SeaweedfsImage =>
        Environment.GetEnvironmentVariable("POLYBUCKET_TEST_SEAWEEDFS_IMAGE") ?? DefaultSeaweedfsImage;
}
