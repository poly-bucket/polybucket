using Core.Models.Catagories;
using Core.Models.Collections;
using Core.Models.Models;
using Core.Models.Roles;
using Core.Models.SystemSettings;
using Core.Models.Tags;
using Core.Models.Users;
using Infrastructure.Data.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Core.Models.RefreshToken.RefreshToken> RefreshTokens { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Core.Models.Files.File> Files { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<SystemSetup> SystemSetups { get; set; }
        public DbSet<ModelComment> ModelComments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ModelTag> ModelTags { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ModelCategory> ModelCategories { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new ModelConfiguration());
            modelBuilder.ApplyConfiguration(new CollectionConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());

            // Configure SystemSetup
            modelBuilder.Entity<SystemSetup>()
                .HasKey(s => s.Id);
        }
    }
}