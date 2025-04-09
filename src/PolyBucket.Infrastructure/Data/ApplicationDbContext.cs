using System;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Models;
using PolyBucket.Core.Models.Auth;
using PolyBucket.Infrastructure.Data.EntityConfigurations;

namespace PolyBucket.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Core.Entities.RefreshToken> RefreshTokens { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Core.Entities.ModelFile> ModelFiles { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<SystemSetup> SystemSetups { get; set; }
        public DbSet<Core.Entities.ModelComment> ModelComments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ModelTag> ModelTags { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ModelCategory> ModelCategories { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
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
            
            // Configure many-to-many relationships
            modelBuilder.Entity<UserFollow>()
                .HasKey(uf => uf.Id);
                
            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowedId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure SystemSetup
            modelBuilder.Entity<SystemSetup>()
                .HasKey(s => s.Id);
        }
    }
} 