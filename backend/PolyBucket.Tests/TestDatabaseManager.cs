using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Settings;
using System.Reflection;


namespace PolyBucket.Tests
{
    public static class TestDatabaseManager
    {
        public static IConfiguration GetTestConfiguration()
        {
            return BuildTestConfiguration();
        }

        private static IConfiguration BuildTestConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true);
            if (!string.IsNullOrEmpty(TestEnvironment.DefaultConnection))
            {
                builder.AddInMemoryCollection(TestDatabaseConfigurationHelper.GetDatabaseKeysFromConnectionString(TestEnvironment.DefaultConnection));
            }

            return builder.Build();
        }

        private static string GetTestConnectionString(IConfiguration configuration)
        {
            var settings = configuration.GetSection("Database").Get<DatabaseSettings>()
                ?? throw new InvalidOperationException(
                    "Test Database section is missing. The test collection must set TestEnvironment.DefaultConnection so database settings are merged.");
            return settings.BuildConnectionString();
        }

        public static async Task EnsureTestDatabaseCreatedAsync()
        {
            var configuration = BuildTestConfiguration();

            var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>()
                ?? throw new InvalidOperationException(
                    "Test Database section is missing. The test collection must set TestEnvironment.DefaultConnection so database settings are merged.");
            var ensurer = new PostgresAppDatabaseEnsurer(Options.Create(databaseSettings));
            await ensurer.EnsureAppDatabaseExistsOrValidateForMigrationAsync(NullLogger<PostgresAppDatabaseEnsurer>.Instance);
            
            var connectionString = GetTestConnectionString(configuration);
            var optionsBuilder = new DbContextOptionsBuilder<PolyBucketDbContext>();
            optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(PolyBucketDbContext).Assembly.GetName().Name!));

            using var context = new PolyBucketDbContext(optionsBuilder.Options);
            
            // Always force database recreation to ensure fresh schema for each test run
            Console.WriteLine("Dropping test database...");
            await context.Database.EnsureDeletedAsync();
            Console.WriteLine("Test database dropped successfully.");
            
            // Use migrations instead of EnsureCreatedAsync to ensure latest schema
            Console.WriteLine("Applying migrations to test database...");
            await context.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");

            // Dynamically seed all permissions from PermissionConstants
            if (!await context.Permissions.AnyAsync())
            {
                var permissionFields = typeof(PermissionConstants).GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(string) && !f.Name.StartsWith("Categories"));
                var permissions = permissionFields.Select(f => new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = (string)f.GetValue(null),
                    Description = f.Name.Replace("_", " "),
                    Category = PermissionConstants.Categories.MODEL_MANAGEMENT, // Default, can be improved
                    IsActive = true,
                    IsSystemPermission = true
                }).ToList();
                // Set correct categories if possible
                foreach (var perm in permissions)
                {
                    if (perm.Name.StartsWith("MODEL_")) perm.Category = PermissionConstants.Categories.MODEL_MANAGEMENT;
                    else if (perm.Name.StartsWith("COLLECTION_")) perm.Category = PermissionConstants.Categories.COLLECTIONS;
                    else if (perm.Name.StartsWith("COMMENT_")) perm.Category = PermissionConstants.Categories.COMMENTS;
                    else if (perm.Name.StartsWith("REPORT_")) perm.Category = PermissionConstants.Categories.REPORTS;
                    else if (perm.Name.StartsWith("API_")) perm.Category = PermissionConstants.Categories.API_ACCESS;
                    else if (perm.Name.StartsWith("STORAGE_")) perm.Category = PermissionConstants.Categories.STORAGE;
                    else if (perm.Name.StartsWith("USER_")) perm.Category = PermissionConstants.Categories.USER_MANAGEMENT;
                }
                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            // Seed roles if they don't exist
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role { Id = Guid.NewGuid(), Name = "User", Description = "Standard user role", Priority = 100, IsSystemRole = true, IsDefault = true, CanBeDeleted = false, IsActive = true },
                    new Role { Id = Guid.NewGuid(), Name = "Moderator", Description = "Content moderation role", Priority = 500, IsSystemRole = true, CanBeDeleted = true, IsActive = true },
                    new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "System administrator role", Priority = 1000, IsSystemRole = true, CanBeDeleted = false, IsActive = true }
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Seed role permissions if they don't exist
            if (!await context.RolePermissions.AnyAsync())
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                var allPermissions = await context.Permissions.ToListAsync();

                if (userRole != null)
                {
                    // Assign all permissions to User role for testing
                    var userPermissions = allPermissions.Select(p => new RolePermission
                    {
                        RoleId = userRole.Id,
                        PermissionId = p.Id,
                        IsGranted = true,
                        GrantedByUserId = null // System granted
                    }).ToList();
                    await context.RolePermissions.AddRangeAsync(userPermissions);
                }

                if (adminRole != null)
                {
                    // Admin gets all permissions
                    var rolePermissions = allPermissions.Select(p => new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = p.Id,
                        IsGranted = true,
                        GrantedByUserId = null // System granted
                    }).ToList();
                    await context.RolePermissions.AddRangeAsync(rolePermissions);
                }

                await context.SaveChangesAsync();
            }


        }

        public static async Task ReseedRolePermissionsIfEmptyAsync(PolyBucketDbContext context)
        {
            if (await context.RolePermissions.AnyAsync())
                return;

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            var allPermissions = await context.Permissions.ToListAsync();

            if (userRole != null && allPermissions.Count > 0)
            {
                var userScopedPermissions = allPermissions.Where(p =>
                    !p.Name.Contains(".any", StringComparison.OrdinalIgnoreCase)
                    && !p.Name.StartsWith("admin.", StringComparison.OrdinalIgnoreCase)
                    && !p.Name.StartsWith("moderation.", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(p.Name, PermissionConstants.MODEL_VIEW_PRIVATE, StringComparison.OrdinalIgnoreCase)).ToList();

                var userPermissions = userScopedPermissions.Select(p => new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = p.Id,
                    IsGranted = true,
                    GrantedByUserId = null
                }).ToList();
                await context.RolePermissions.AddRangeAsync(userPermissions);
            }

            if (adminRole != null && allPermissions.Count > 0)
            {
                var rolePermissions = allPermissions.Select(p => new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = p.Id,
                    IsGranted = true,
                    GrantedByUserId = null
                }).ToList();
                await context.RolePermissions.AddRangeAsync(rolePermissions);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedRequiredDataAsync(PolyBucketDbContext context)
        {
            // Seed roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Admin",
                        Description = "Administrator role",
                        Priority = 100,
                        IsSystemRole = true,
                        IsDefault = false,
                        IsActive = true
                    },
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "User",
                        Description = "Default user role",
                        Priority = 1,
                        IsSystemRole = true,
                        IsDefault = true,
                        IsActive = true
                    },
                    new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Moderator",
                        Description = "Moderator role",
                        Priority = 50,
                        IsSystemRole = true,
                        IsDefault = false,
                        IsActive = true
                    }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Seed permissions
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new[]
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = "MODEL_CREATE",
                        Description = "Create models",
                        Category = "Models"
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = "MODEL_UPDATE",
                        Description = "Update models",
                        Category = "Models"
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = "MODEL_DELETE",
                        Description = "Delete models",
                        Category = "Models"
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = "MODEL_DELETE_ANY",
                        Description = "Delete any model",
                        Category = "Models"
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = "USER_MANAGE",
                        Description = "Manage users",
                        Category = "Users"
                    }
                };

                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            // Seed role permissions
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            var moderatorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Moderator");

            if (adminRole != null && !await context.RolePermissions.AnyAsync())
            {
                var allPermissions = await context.Permissions.ToListAsync();
                var rolePermissions = allPermissions.Select(p => new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = p.Id,
                    IsGranted = true,
                    GrantedByUserId = null // System granted
                }).ToList();

                await context.RolePermissions.AddRangeAsync(rolePermissions);
                await context.SaveChangesAsync();
            }
        }

        public static async Task CleanupTestDatabaseAsync()
        {
            var configuration = BuildTestConfiguration();

            var connectionString = GetTestConnectionString(configuration);
            var optionsBuilder = new DbContextOptionsBuilder<PolyBucketDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new PolyBucketDbContext(optionsBuilder.Options);
            
            // Clear all data but keep the schema
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Roles\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Permissions\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RolePermissions\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"UserPermissions\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Models\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ModelVersions\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ModelFiles\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Collections\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"CollectionModels\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Comments\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RefreshTokens\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"UserSettings\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SystemSetups\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Reports\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Filaments\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Printers\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Categories\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"FederationNodes\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"FederationSyncs\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ModelModeration\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ModerationAuditLogs\" CASCADE");
            
            // Reset sequences
            await context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"__EFMigrationsHistory_Id_seq\" RESTART WITH 1");

            // Re-seed required data
            await SeedRequiredDataAsync(context);
        }

        public static void ConfigureTestServices(IServiceCollection services)
        {
            var configuration = BuildTestConfiguration();

            var connectionString = GetTestConnectionString(configuration);
            services.AddDbContext<PolyBucketDbContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(PolyBucketDbContext).Assembly.GetName().Name!)));

            // Add other services as needed for testing
            services.AddLogging();
            services.AddHttpClient();
        }
    }
} 