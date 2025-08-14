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
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using ReportsDomain = PolyBucket.Api.Features.Reports.Domain;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Data
{
    public class PolyBucketDbContext(DbContextOptions<PolyBucketDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserLogin> UserLogins { get; set; } = null!;
        public DbSet<UserSettings> UserSettings { get; set; } = null!;
        public DbSet<Printer> Printers { get; set; } = null!;
        public DbSet<CommentDomain.Comment> Comments { get; set; } = null!;
        public DbSet<Model> Models { get; set; } = null!;
        public DbSet<ModelFile> ModelFiles { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<ModelVersion> ModelVersions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Filament> Filaments { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<SystemSetup> SystemSetups { get; set; } = null!;
        public DbSet<FontAwesomeSettings> FontAwesomeSettings { get; set; } = null!;
        public DbSet<FileTypeSettings> FileTypeSettings { get; set; } = null!;
        public DbSet<Collection> Collections { get; set; } = null!;
        public DbSet<CollectionModel> CollectionModels { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
        public DbSet<ExternalAuthProvider> ExternalAuthProviders { get; set; } = null!;
        public DbSet<TwoFactorAuthDomain.TwoFactorAuth> TwoFactorAuths { get; set; } = null!;
        public DbSet<TwoFactorAuthDomain.BackupCode> BackupCodes { get; set; } = null!;
        public DbSet<ModelPreview> ModelPreviews { get; set; } = null!;
        public DbSet<ModerationAuditLog> ModerationAuditLogs { get; set; } = null!;
        
        // ACL System
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<UserPermission> UserPermissions { get; set; } = null!;
        
        // Federation System
        public DbSet<FederationSettings> FederationSettings { get; set; } = null!;
        public DbSet<FederatedInstance> FederatedInstances { get; set; } = null!;
        public DbSet<FederatedModel> FederatedModels { get; set; } = null!;
        public DbSet<FederationHandshake> FederationHandshakes { get; set; } = null!;
        public DbSet<FederationAuditLog> FederationAuditLogs { get; set; } = null!;
        public DbSet<ReportsDomain.Report> Reports { get; set; } = null!;
        
        // Theme Management
        public DbSet<Theme> Themes { get; set; } = null!;

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

            // Model Version Configuration
            modelBuilder.Entity<ModelVersion>()
                .HasOne(v => v.Model)
                .WithMany(m => m.Versions)
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ModelVersion>()
                .HasIndex(v => new { v.ModelId, v.VersionNumber })
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

            // Federation Configuration
            modelBuilder.Entity<FederatedModel>()
                .HasOne(f => f.FederatedInstance)
                .WithMany(i => i.SharedModels)
                .HasForeignKey(f => f.FederatedInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FederatedModel>()
                .HasOne(f => f.LocalModel)
                .WithMany()
                .HasForeignKey(f => f.LocalModelId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FederationHandshake>()
                .HasOne(h => h.FederatedInstance)
                .WithMany(i => i.Handshakes)
                .HasForeignKey(h => h.FederatedInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FederationAuditLog>()
                .HasOne(a => a.FederatedInstance)
                .WithMany(i => i.AuditLogs)
                .HasForeignKey(a => a.FederatedInstanceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FederationAuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.ApplyConfiguration(new FilamentMap());
            // modelBuilder.ApplyConfiguration(new UserMap());
            // modelBuilder.ApplyConfiguration(new UserLoginMap());
            
            // Theme Configuration
            modelBuilder.Entity<Theme>()
                .HasOne(t => t.Colors)
                .WithOne(c => c.Theme)
                .HasForeignKey<ThemeColors>(c => c.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Theme>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
} 