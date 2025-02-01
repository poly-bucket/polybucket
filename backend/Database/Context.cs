using Core.Models;
using Core.Models.Comments;
using Core.Models.Enumerations;
using Core.Models.Files;
using Core.Models.Interfaces;
using Core.Models.Models;
using Core.Models.Users;
using Core.Models.Users.Settings;
using Core.Plugins.Reports;
using Database.Maps.Comments;
using Database.Maps.Files;
using Database.Maps.Models;
using Database.Maps.Reports;
using Database.Maps.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Database;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    public static string ConnectionString =>
        $"Host={Environment.GetEnvironmentVariable("DB_HOST")};"
        + $"Port={Environment.GetEnvironmentVariable("DB_PORT")};"
        + $"Username={Environment.GetEnvironmentVariable("DB_USER")};"
        + $"Password={Environment.GetEnvironmentVariable("DB_PASS")};"
        + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
        + $"SSL Mode=none;";

    public DbSet<Core.Models.Users.User> Users { get; set; } = null!;
    public DbSet<Core.Models.Users.UserLogin> UserLogins { get; set; } = null!;
    public DbSet<Core.Models.Users.Settings.UserSettings> UserSettings { get; set; } = null!;
    public DbSet<Core.Models.Models.Model> Models { get; set; } = null!;
    public DbSet<Core.Models.Files.File> Files { get; set; } = null!;
    public DbSet<Core.Models.Models.PrintSettings> PrintSettings { get; set; } = null!;
    public DbSet<Core.Models.Comments.Comment> Comments { get; set; } = null!;
    public DbSet<Core.Plugins.Reports.Report> Reports { get; set; } = null!;
    public DbSet<Core.Models.Printers.Printer> Printers { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            ServerVersion version = ServerVersion.AutoDetect(ConnectionString);
            optionsBuilder.UseMySql(ConnectionString, version);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        JsonSerializerSettings settings = new()
        {
            Formatting = Formatting.None,
        };

        // Users
        modelBuilder.Entity<Core.Models.Users.User>()
            .Property(prop => prop.Role)
            .HasConversion(new EnumToStringConverter<Core.Models.Users.UserRole>());

        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.ApplyConfiguration(new UserLoginMap());
        modelBuilder.ApplyConfiguration(new UserSettingsMap());

        // Models
        modelBuilder.ApplyConfiguration(new ModelMap());
        modelBuilder.ApplyConfiguration(new PrintSettingsMap());

        // Files
        modelBuilder.ApplyConfiguration(new FileMap());

        // Comments
        modelBuilder.ApplyConfiguration(new CommentMap());

        // Reports
        modelBuilder.ApplyConfiguration(new ReportMap());
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (IEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.Id = Guid.NewGuid().ToString();
            }
        }

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (IEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.Id = Guid.NewGuid().ToString();
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}