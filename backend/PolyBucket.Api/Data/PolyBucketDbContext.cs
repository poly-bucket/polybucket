using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Printers.Domain;
using CommentDomain = PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Filaments.Repository;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Data
{
    public class PolyBucketDbContext : DbContext
    {
        public PolyBucketDbContext(DbContextOptions<PolyBucketDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserLogin> UserLogins { get; set; } = null!;
        public DbSet<UserSettings> UserSettings { get; set; } = null!;
        public DbSet<Printer> Printers { get; set; } = null!;
        public DbSet<CommentDomain.Comment> Comments { get; set; } = null!;
        public DbSet<Model> Models { get; set; } = null!;
        public DbSet<Filament> Filaments { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<Collection> Collections { get; set; } = null!;
        public DbSet<CollectionModel> CollectionModels { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
        public DbSet<ExternalAuthProvider> ExternalAuthProviders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CollectionModel>()
                .HasKey(cm => new { cm.CollectionId, cm.ModelId });

            modelBuilder.Entity<CollectionModel>()
                .HasOne(cm => cm.Collection)
                .WithMany(c => c.CollectionModels)
                .HasForeignKey(cm => cm.CollectionId);

            modelBuilder.Entity<CollectionModel>()
                .HasOne(cm => cm.Model)
                .WithMany() // Assuming a model can be in many collections, but Model entity doesn't have a direct navigation back to CollectionModel
                .HasForeignKey(cm => cm.ModelId);
                
            modelBuilder.ApplyConfiguration(new FilamentMap());
            // modelBuilder.ApplyConfiguration(new UserMap());
            // modelBuilder.ApplyConfiguration(new UserLoginMap());
        }
    }
} 