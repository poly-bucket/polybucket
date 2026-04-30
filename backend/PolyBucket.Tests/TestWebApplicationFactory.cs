using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using PolyBucket.Api.Settings;

namespace PolyBucket.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                context.HostingEnvironment.EnvironmentName = "Test";
                
                var testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json");
                if (File.Exists(testConfigPath))
                {
                    config.AddJsonFile(testConfigPath, optional: false);
                }
                else
                {
                    var fromConn = TestDatabaseConfigurationHelper.GetDatabaseKeysFromConnectionString(
                        "Host=127.0.0.1;Port=5432;Database=polybucket_test;Username=postgres;Password=postgres;SSL Mode=Disable");
                    var inline = new Dictionary<string, string?>(fromConn)
                    {
                        ["AppSettings:Security:JwtSecret"] = "test-jwt-secret-key-for-testing-purposes-only-32-chars",
                        ["AppSettings:Security:JwtIssuer"] = "polybucket-test-api",
                        ["AppSettings:Security:JwtAudience"] = "polybucket-test-client",
                        ["AppSettings:Security:AccessTokenExpiryMinutes"] = "60",
                        ["AppSettings:Security:RefreshTokenExpiryDays"] = "7",
                        ["Database:SkipHostDatabaseInitialization"] = "true"
                    };
                    config.AddInMemoryCollection(inline);
                }

                if (string.IsNullOrEmpty(TestEnvironment.DefaultConnection))
                {
                    throw new InvalidOperationException(
                        "TestEnvironment.DefaultConnection is not set. The Test collection fixture must start PostgreSQL and assign the connection string first.");
                }
                if (string.IsNullOrWhiteSpace(TestEnvironment.StorageEndpoint)
                    || !TestEnvironment.StoragePort.HasValue
                    || string.IsNullOrWhiteSpace(TestEnvironment.StorageAccessKey)
                    || string.IsNullOrWhiteSpace(TestEnvironment.StorageSecretKey)
                    || string.IsNullOrWhiteSpace(TestEnvironment.StorageBucketName)
                    || !TestEnvironment.StorageUseSsl.HasValue)
                {
                    throw new InvalidOperationException(
                        "TestEnvironment storage settings are not set. The Test collection fixture must start MinIO and assign storage settings first.");
                }

                var dbFromContainer = new Dictionary<string, string?>(TestDatabaseConfigurationHelper.GetDatabaseKeysFromConnectionString(TestEnvironment.DefaultConnection))
                {
                    ["Database:SkipHostDatabaseInitialization"] = "true"
                };
                config.AddInMemoryCollection(dbFromContainer);

                var storageFromContainer = new Dictionary<string, string?>
                {
                    ["Storage:Endpoint"] = TestEnvironment.StorageEndpoint,
                    ["Storage:Port"] = TestEnvironment.StoragePort.Value.ToString(),
                    ["Storage:AccessKey"] = TestEnvironment.StorageAccessKey,
                    ["Storage:SecretKey"] = TestEnvironment.StorageSecretKey,
                    ["Storage:BucketName"] = TestEnvironment.StorageBucketName,
                    ["Storage:UseSSL"] = TestEnvironment.StorageUseSsl.Value.ToString()
                };
                config.AddInMemoryCollection(storageFromContainer);

                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Security:JwtSecret"] = "test-jwt-secret-key-for-testing-purposes-only-32-chars",
                    ["AppSettings:Security:JwtIssuer"] = "polybucket-test-api",
                    ["AppSettings:Security:JwtAudience"] = "polybucket-test-client",
                    ["AppSettings:Security:AccessTokenExpiryMinutes"] = "60",
                    ["AppSettings:Security:RefreshTokenExpiryDays"] = "7"
                });
            });
            
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PolyBucketDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var configuration = TestDatabaseManager.GetTestConfiguration();
                var settings = configuration.GetSection("Database").Get<DatabaseSettings>()
                    ?? throw new InvalidOperationException("Database section is missing. Ensure the Test collection fixture ran.");
                var connectionString = settings.BuildConnectionString();
                
                services.AddDbContext<PolyBucketDbContext>(options =>
                    options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(PolyBucketDbContext).Assembly.GetName().Name!)));

                services.AddLogging();
                services.AddHttpClient();
            });
        }
    }
}
