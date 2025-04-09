using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions.Database
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations<T>(this IApplicationBuilder app) where T : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<T>();
            
            try
            {
                var pendingMigrations = context.Database.GetPendingMigrations();
                if (pendingMigrations.Any())
                {
                    context.Database.Migrate();
                    System.Console.WriteLine("Applied pending database migrations.");
                }
                else
                {
                    System.Console.WriteLine("No pending migrations to apply.");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
                throw;
            }
        }
    }
} 