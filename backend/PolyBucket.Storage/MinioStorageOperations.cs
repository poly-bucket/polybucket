using Minio;
using Minio.DataModel.Args;
using PolyBucket.Storage.Settings;

namespace PolyBucket.Storage;

public static class MinioStorageOperations
{
    public static IMinioClient CreateClient(StorageSettings settings) =>
        new MinioClient()
            .WithEndpoint(settings.Endpoint, settings.Port)
            .WithCredentials(settings.AccessKey, settings.SecretKey)
            .WithSSL(settings.UseSSL)
            .Build();

    public static async Task EnsureBucketExistsAsync(
        IMinioClient client,
        string bucketName,
        CancellationToken cancellationToken = default)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
        var found = await client.BucketExistsAsync(existsArgs, cancellationToken).ConfigureAwait(false);
        if (!found)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
            await client.MakeBucketAsync(makeBucketArgs, cancellationToken).ConfigureAwait(false);
        }
    }

    public static async Task TestConnectivityAsync(
        StorageSettings settings,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient(settings);
        await EnsureBucketExistsAsync(client, settings.BucketName, cancellationToken).ConfigureAwait(false);
    }
}
