using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Minio;
using Minio.DataModel.Args;
using PolyBucket.Storage.Settings;
using Microsoft.Extensions.Configuration;

namespace PolyBucket.Storage.Tests;

[CollectionDefinition("StorageTestCollection")]
public class StorageTestCollection : ICollectionFixture<StorageTestCollectionFixture>
{
}

public class StorageTestCollectionFixture : IAsyncLifetime
{
    private const string MinioRootUser = "minioadmin";
    private const string MinioRootPassword = "minioadmin";
    private const string MinioTestBucketName = "polybucket-storage-test";
    private const ushort MinioPort = 9000;

    private IContainer? _minioContainer;

    public IConfiguration Configuration { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _minioContainer = new ContainerBuilder(StorageTestContainerImages.MinioImage)
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithEnvironment("MINIO_ROOT_USER", MinioRootUser)
            .WithEnvironment("MINIO_ROOT_PASSWORD", MinioRootPassword)
            .WithCommand("server", "/data", "--console-address", ":9001")
            .WithPortBinding(MinioPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(MinioPort))
            .Build();

        using var startCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await _minioContainer.StartAsync(startCts.Token);

        StorageTestEnvironment.Endpoint = _minioContainer.Hostname;
        StorageTestEnvironment.Port = _minioContainer.GetMappedPublicPort(MinioPort);
        StorageTestEnvironment.AccessKey = MinioRootUser;
        StorageTestEnvironment.SecretKey = MinioRootPassword;
        StorageTestEnvironment.BucketName = MinioTestBucketName;

        var minioClient = new MinioClient()
            .WithEndpoint(StorageTestEnvironment.Endpoint, StorageTestEnvironment.Port.Value)
            .WithCredentials(MinioRootUser, MinioRootPassword)
            .WithSSL(false)
            .Build();

        if (!await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(MinioTestBucketName), startCts.Token))
        {
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(MinioTestBucketName), startCts.Token);
        }

        Configuration = BuildConfiguration();
    }

    public async Task DisposeAsync()
    {
        if (_minioContainer is not null)
        {
            await _minioContainer.DisposeAsync();
        }
    }

    public static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "MinIO",
                ["Storage:Endpoint"] = StorageTestEnvironment.Endpoint,
                ["Storage:Port"] = StorageTestEnvironment.Port?.ToString(),
                ["Storage:AccessKey"] = StorageTestEnvironment.AccessKey,
                ["Storage:SecretKey"] = StorageTestEnvironment.SecretKey,
                ["Storage:BucketName"] = StorageTestEnvironment.BucketName,
                ["Storage:UseSSL"] = "false"
            })
            .Build();
    }

    public static IStorageService CreateStorageService(string provider = StorageProviderIds.Silo)
    {
        var settings = new StorageSettings
        {
            Provider = provider,
            Endpoint = StorageTestEnvironment.Endpoint ?? "localhost",
            Port = StorageTestEnvironment.Port ?? 9000,
            AccessKey = StorageTestEnvironment.AccessKey ?? MinioRootUser,
            SecretKey = StorageTestEnvironment.SecretKey ?? MinioRootPassword,
            BucketName = StorageTestEnvironment.BucketName ?? MinioTestBucketName,
            UseSSL = false,
            ExternalEndpoint = StorageTestEnvironment.Endpoint,
            ExternalPort = StorageTestEnvironment.Port,
            ExternalUseSSL = false
        };

        return new MinioStorageService(settings);
    }
}
