using System.Collections.Generic;
using System.IO;
using System.Threading;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using PolyBucket.Api.Data;
using PolyBucket.Api.Settings;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
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
        private const string TestBucketName = "polybucket-test-uploads";
        private const string S3AccessKey = "polybucket";
        private const string S3SecretKey = "polybucketsecret";
        private const ushort S3Port = 8333;
        private PostgreSqlContainer? _postgresContainer;
        private IContainer? _seaweedfsContainer;

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
            _seaweedfsContainer = new ContainerBuilder()
                .WithImage(TestContainerImages.SeaweedfsImage)
                .WithImagePullPolicy(PullPolicy.Missing)
                .WithEnvironment("AWS_ACCESS_KEY_ID", S3AccessKey)
                .WithEnvironment("AWS_SECRET_ACCESS_KEY", S3SecretKey)
                .WithCommand("server", "-dir=/data", "-s3", "-s3.port=8333", "-ip=0.0.0.0")
                .WithPortBinding(S3Port, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(S3Port))
                .Build();

            using var startCts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            await _postgresContainer.StartAsync(startCts.Token).ConfigureAwait(false);
            await _seaweedfsContainer.StartAsync(startCts.Token).ConfigureAwait(false);

            var builder = new NpgsqlConnectionStringBuilder(_postgresContainer.GetConnectionString())
            {
                Database = "polybucket_test",
                SslMode = SslMode.Disable
            };
            TestEnvironment.DefaultConnection = builder.ConnectionString;
            TestEnvironment.StorageEndpoint = _seaweedfsContainer.Hostname;
            TestEnvironment.StoragePort = _seaweedfsContainer.GetMappedPublicPort(S3Port);
            TestEnvironment.StorageAccessKey = S3AccessKey;
            TestEnvironment.StorageSecretKey = S3SecretKey;
            TestEnvironment.StorageBucketName = TestBucketName;
            TestEnvironment.StorageUseSsl = false;

            await EnsureTestBucketExistsAsync(startCts.Token).ConfigureAwait(false);

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
            if (_seaweedfsContainer is not null)
            {
                await _seaweedfsContainer.DisposeAsync().ConfigureAwait(false);
            }

            if (_postgresContainer is not null)
            {
                await _postgresContainer.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task EnsureTestBucketExistsAsync(CancellationToken cancellationToken)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = $"http://{TestEnvironment.StorageEndpoint}:{TestEnvironment.StoragePort}",
                ForcePathStyle = true,
                UseHttp = true
            };
            using var client = new AmazonS3Client(TestEnvironment.StorageAccessKey, TestEnvironment.StorageSecretKey, config);
            try
            {
                await client.PutBucketAsync(new PutBucketRequest { BucketName = TestBucketName }, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
            {
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
