using Api;
using Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set test environment variables
        Environment.SetEnvironmentVariable("JWT_SECRET", "test_jwt_secret_key_for_testing_purposes_only");
        Environment.SetEnvironmentVariable("MYSQL_HOST", "localhost");
        Environment.SetEnvironmentVariable("MYSQL_PORT", "3306");
        Environment.SetEnvironmentVariable("MYSQL_DATABASE", "test_db");
        Environment.SetEnvironmentVariable("MYSQL_USER", "test_user");
        Environment.SetEnvironmentVariable("MYSQL_PASSWORD", "test_password");

        builder.ConfigureServices(services =>
        {
            // Remove the app's database context registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<Context>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add database context using an in-memory database for testing
            services.AddDbContext<Context>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Create a new service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<Context>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            // Seed the database with test data
            try
            {
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred seeding the database.", ex);
            }
        });
    }

    public override ValueTask DisposeAsync()
    {
        // Clean up test environment variables
        Environment.SetEnvironmentVariable("JWT_SECRET", null);
        Environment.SetEnvironmentVariable("MYSQL_HOST", null);
        Environment.SetEnvironmentVariable("MYSQL_PORT", null);
        Environment.SetEnvironmentVariable("MYSQL_DATABASE", null);
        Environment.SetEnvironmentVariable("MYSQL_USER", null);
        Environment.SetEnvironmentVariable("MYSQL_PASSWORD", null);

        return base.DisposeAsync();
    }

    private void SeedTestData(Context db)
    {
        var testUser = new Core.Models.Users.User
        {
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8", // This is "password123" hashed
            Salt = "testsalt",
            Role = Core.Models.Users.UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(testUser);
        db.SaveChanges();
    }
}