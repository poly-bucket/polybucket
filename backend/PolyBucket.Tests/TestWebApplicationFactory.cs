using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api;
using PolyBucket.Api.Data;
using Microsoft.Extensions.Configuration;
using System.IO;

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
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = "Host=127.0.0.1;Port=5432;Database=polybucket_test;Username=postgres;Password=postgres;SSL Mode=Disable",
                        ["AppSettings:Security:JwtSecret"] = "test-jwt-secret-key-for-testing-purposes-only-32-chars",
                        ["AppSettings:Security:JwtIssuer"] = "polybucket-test-api",
                        ["AppSettings:Security:JwtAudience"] = "polybucket-test-client",
                        ["AppSettings:Security:AccessTokenExpiryMinutes"] = "60",
                        ["AppSettings:Security:RefreshTokenExpiryDays"] = "7",
                        ["Database:SkipHostDatabaseInitialization"] = "true"
                    });
                }

                if (string.IsNullOrEmpty(TestEnvironment.DefaultConnection))
                {
                    throw new InvalidOperationException(
                        "TestEnvironment.DefaultConnection is not set. The Test collection fixture must start PostgreSQL and assign the connection string first.");
                }

                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = TestEnvironment.DefaultConnection,
                        ["Database:SkipHostDatabaseInitialization"] = "true"
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
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Test DefaultConnection is missing. Ensure the Test collection fixture ran.");
                }
                
                services.AddDbContext<PolyBucketDbContext>(options =>
                    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("PolyBucket.Api")));

                services.AddLogging();
                services.AddHttpClient();
            });
        }
    }
} 