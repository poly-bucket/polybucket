using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Npgsql;
using PolyBucket.Api.Data;
using PolyBucket.Api.Settings;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Testcontainers.PostgreSql;
using Xunit;
using LogAbstractions = Microsoft.Extensions.Logging.Abstractions;

namespace PolyBucket.Tests
{
    [CollectionDefinition("TestCollection")]
    public class TestCollection : ICollectionFixture<TestCollectionFixture>
    {
    }

    public class TestCollectionFixture : IAsyncLifetime
    {
        private const string MinioTestBucketName = "polybucket-test-uploads";
        private const string MinioRootUser = "minioadmin";
        private const string MinioRootPassword = "minioadmin";
        private const ushort MinioPort = 9000;
        private PostgreSqlContainer? _postgresContainer;
        private IContainer? _minioContainer;

        public async Task InitializeAsync()
        {
            Environment.SetEnvironmentVariable("AppSettings__Security__JwtSecret", "test-jwt-secret-key-for-testing-purposes-only-32-chars");
            Environment.SetEnvironmentVariable("AppSettings__Security__JwtIssuer", "polybucket-test-api");
            Environment.SetEnvironmentVariable("AppSettings__Security__JwtAudience", "polybucket-test-client");

            _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("postgres")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
            _minioContainer = new ContainerBuilder()
                .WithImage("minio/minio:RELEASE.2025-04-08T15-41-24Z")
                .WithEnvironment("MINIO_ROOT_USER", MinioRootUser)
                .WithEnvironment("MINIO_ROOT_PASSWORD", MinioRootPassword)
                .WithCommand("server", "/data", "--console-address", ":9001")
                .WithPortBinding(MinioPort, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(MinioPort))
                .Build();

            using var startCts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            await _postgresContainer.StartAsync(startCts.Token).ConfigureAwait(false);
            await _minioContainer.StartAsync(startCts.Token).ConfigureAwait(false);

            var builder = new NpgsqlConnectionStringBuilder(_postgresContainer.GetConnectionString())
            {
                Database = "polybucket_test",
                SslMode = SslMode.Disable
            };
            TestEnvironment.DefaultConnection = builder.ConnectionString;
            TestEnvironment.StorageEndpoint = _minioContainer.Hostname;
            TestEnvironment.StoragePort = _minioContainer.GetMappedPublicPort(MinioPort);
            TestEnvironment.StorageAccessKey = MinioRootUser;
            TestEnvironment.StorageSecretKey = MinioRootPassword;
            TestEnvironment.StorageBucketName = MinioTestBucketName;
            TestEnvironment.StorageUseSsl = false;

            var minioClient = new MinioClient()
                .WithEndpoint(TestEnvironment.StorageEndpoint, TestEnvironment.StoragePort.Value)
                .WithCredentials(TestEnvironment.StorageAccessKey, TestEnvironment.StorageSecretKey)
                .WithSSL(false)
                .Build();
            var bucketExists = await minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(MinioTestBucketName),
                startCts.Token).ConfigureAwait(false);
            if (!bucketExists)
            {
                await minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(MinioTestBucketName),
                    startCts.Token).ConfigureAwait(false);
            }

            var configuration = BuildConfigurationForEnsurer();
            var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>()
                ?? throw new InvalidOperationException("Database section is missing for test ensurer.");
            var ensurer = new PostgresAppDatabaseEnsurer(Options.Create(databaseSettings));
            var logger = LogAbstractions.NullLogger<PostgresAppDatabaseEnsurer>.Instance;
            await ensurer.EnsureAppDatabaseExistsOrValidateForMigrationAsync(logger).ConfigureAwait(false);
            await TestDatabaseManager.EnsureTestDatabaseCreatedAsync().ConfigureAwait(false);
        }

        public async Task DisposeAsync()
        {
            if (_minioContainer is not null)
            {
                await _minioContainer.DisposeAsync().ConfigureAwait(false);
            }

            if (_postgresContainer is not null)
            {
                await _postgresContainer.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static IConfiguration BuildConfigurationForEnsurer()
        {
            var dbKeys = new Dictionary<string, string?>(TestDatabaseConfigurationHelper.GetDatabaseKeysFromConnectionString(TestEnvironment.DefaultConnection!))
            {
                ["Database:EnsureDatabaseCreated"] = "true",
                ["Database:MaintenanceDatabase"] = "postgres"
            };
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddInMemoryCollection(dbKeys)
                .Build();
        }
    }
}
