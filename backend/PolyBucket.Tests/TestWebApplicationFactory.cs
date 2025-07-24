using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                
                // Add test configuration from the correct path
                var testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json");
                if (File.Exists(testConfigPath))
                {
                    config.AddJsonFile(testConfigPath, optional: false);
                }
                else
                {
                    // Fallback to default test configuration
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5433;Database=polybucket_test;Username=postgres;Password=postgres;",
                        ["AppSettings:Security:JwtSecret"] = "test-jwt-secret-key-for-testing-purposes-only-32-chars",
                        ["AppSettings:Security:JwtIssuer"] = "polybucket-test-api",
                        ["AppSettings:Security:JwtAudience"] = "polybucket-test-client",
                        ["AppSettings:Security:AccessTokenExpiryMinutes"] = "60",
                        ["AppSettings:Security:RefreshTokenExpiryDays"] = "7"
                    });
                }
            });
            
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PolyBucketDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add test database context without migrations
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Test.json", optional: true)
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5433;Database=polybucket_test;Username=postgres;Password=postgres;",
                        ["AppSettings:Security:JwtSecret"] = "test-jwt-secret-key-for-testing-purposes-only-32-chars",
                        ["AppSettings:Security:JwtIssuer"] = "polybucket-test-api",
                        ["AppSettings:Security:JwtAudience"] = "polybucket-test-client",
                        ["AppSettings:Security:AccessTokenExpiryMinutes"] = "60",
                        ["AppSettings:Security:RefreshTokenExpiryDays"] = "7"
                    })
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                
                services.AddDbContext<PolyBucketDbContext>(options =>
                    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("PolyBucket.Api")));

                // Add other services as needed for testing
                services.AddLogging();
                services.AddHttpClient();
            });
        }
    }
} 