using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Printers.Domain;
using CommentDomain = PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Filaments.Repository;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.ACL.Domain;

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
        public DbSet<ModelFile> ModelFiles { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Filament> Filaments { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<Collection> Collections { get; set; } = null!;
        public DbSet<CollectionModel> CollectionModels { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
        public DbSet<ExternalAuthProvider> ExternalAuthProviders { get; set; } = null!;
        public DbSet<ModelPreview> ModelPreviews { get; set; } = null!;
        public DbSet<ModerationAuditLog> ModerationAuditLogs { get; set; } = null!;
        
        // ACL System
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<UserPermission> UserPermissions { get; set; } = null!;

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

            modelBuilder.Entity<ModelPreview>()
                .HasOne(p => p.Model)
                .WithMany()
                .HasForeignKey(p => p.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ModelPreview>()
                .HasIndex(p => new { p.ModelId, p.Size })
                .IsUnique();

            // ACL Configuration
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            modelBuilder.Entity<UserPermission>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId);

            modelBuilder.Entity<Role>()
                .HasOne(r => r.ParentRole)
                .WithMany(r => r.ChildRoles)
                .HasForeignKey(r => r.ParentRoleId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.ApplyConfiguration(new FilamentMap());
            // modelBuilder.ApplyConfiguration(new UserMap());
            // modelBuilder.ApplyConfiguration(new UserLoginMap());
        }
    }
} 