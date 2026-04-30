namespace PolyBucket.Tests;

public static class TestEnvironment
{
    public static string? DefaultConnection { get; set; }
    public static string? StorageEndpoint { get; set; }
    public static int? StoragePort { get; set; }
    public static string? StorageAccessKey { get; set; }
    public static string? StorageSecretKey { get; set; }
    public static string? StorageBucketName { get; set; }
    public static bool? StorageUseSsl { get; set; }
}
