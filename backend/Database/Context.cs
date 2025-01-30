using Core.Models;
using Core.Models.Filaments;
using Core.Models.Files;
using Core.Models.Models;
using Core.Models.Printers;
using Core.Models.Users;
using Database.Maps.Comments;
using Database.Maps.Filaments;
using Database.Maps.Files;
using Database.Maps.Models;
using Database.Maps.Printers;
using Database.Maps.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Database
{
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

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<UserLogin> UserLogins { get; set; } = null!;

        public DbSet<Printer> Printers { get; set; } = null!;

        public DbSet<Filament> Filaments { get; set; } = null!;

        public DbSet<Core.Models.Files.File> Files { get; set; } = null!;

        public DbSet<Model> Models { get; set; } = null!;

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
            modelBuilder.Entity<User>().Property(prop => prop.Role).HasConversion(new EnumToStringConverter<UserRole>());

            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new UserLoginMap());

            // Models
            modelBuilder.ApplyConfiguration(new ModelMap());
            modelBuilder.ApplyConfiguration(new PrintSettingsMap());

            // Files
            modelBuilder.ApplyConfiguration(new FileMap());

            // Printers and Filaments
            modelBuilder.ApplyConfiguration(new PrinterMap());
            modelBuilder.ApplyConfiguration(new FilamentMap());

            // Comments
            modelBuilder.ApplyConfiguration(new CommentMap());
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.Id = Guid.NewGuid();
                }
            }

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.Id = Guid.NewGuid();
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}