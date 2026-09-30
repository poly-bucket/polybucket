using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Printers.Domain;
using CommentDomain = PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.RecordModelView.Domain;
using PolyBucket.Api.Features.Models.RecordModelDownload.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Filaments.Repository;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using PolyBucket.Api.Features.Notifications.Domain;
using ReportsDomain = PolyBucket.Api.Features.Reports.Domain;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Data
{
    public class PolyBucketDbContext(DbContextOptions<PolyBucketDbContext> options) : DbContext(options), IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
        public DbSet<EmailMessage> EmailMessages { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserLogin> UserLogins { get; set; } = null!;
        public DbSet<UserSettings> UserSettings { get; set; } = null!;
        public DbSet<UserAuditLog> UserAuditLogs { get; set; } = null!;
        public DbSet<Printer> Printers { get; set; } = null!;
        public DbSet<CommentDomain.Comment> Comments { get; set; } = null!;
        public DbSet<CommentDomain.EnhancedComment> EnhancedComments { get; set; } = null!;
        public DbSet<CommentDomain.CommentReaction> CommentReactions { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Model> Models { get; set; } = null!;
        public DbSet<ModelFile> ModelFiles { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<ModelViewDedup> ModelViewDedups { get; set; } = null!;
        public DbSet<ModelDownloadDedup> ModelDownloadDedups { get; set; } = null!;
        public DbSet<ModelVersion> ModelVersions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Filament> Filaments { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<SystemSetup> SystemSetups { get; set; } = null!;
        public DbSet<FontAwesomeSettings> FontAwesomeSettings { get; set; } = null!;
        public DbSet<FileTypeSettings> FileTypeSettings { get; set; } = null!;
        public DbSet<ModelSettings> ModelSettings { get; set; } = null!;
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
        public DbSet<ModelModerationRecord> ModelModerationRecords { get; set; } = null!;
        
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

            // Collection Configuration
            modelBuilder.Entity<Collection>()
                .Property(c => c.Favorite)
                .HasDefaultValue(false);

            modelBuilder.Entity<Collection>()
                .Property(c => c.DisplayOrder)
                .HasDefaultValue(0);

            modelBuilder.Entity<Collection>()
                .HasOne(c => c.Owner)
                .WithMany()
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Collection>()
                .HasIndex(c => new { c.OwnerId, c.Favorite, c.DisplayOrder })
                .HasDatabaseName("IX_Collections_OwnerId_Favorite_DisplayOrder");

            modelBuilder.Entity<Collection>()
                .HasIndex(c => new { c.OwnerId, c.CreatedAt })
                .IsDescending(false, true)
                .HasFilter("\"DeletedAt\" IS NULL")
                .HasDatabaseName("IX_Collections_OwnerId_CreatedAt_Active");

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

            modelBuilder.Entity<ModelPreview>()
                .HasIndex(p => new { p.Status, p.NextAttemptAt });

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
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(u => u.BannedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.BannedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.BannedAt)
                    .IsDescending(true)
                    .HasFilter("\"IsBanned\" = true")
                    .HasDatabaseName("IX_Users_BannedAt_WhenBanned");
            });

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

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.GrantedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.GrantedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.GrantedByUser)
                .WithMany()
                .HasForeignKey(up => up.GrantedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserLogin>()
                .HasOne(l => l.User)
                .WithMany(u => u.Logins)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Model>(entity =>
            {
                entity.HasOne(m => m.Author)
                    .WithMany()
                    .HasForeignKey(m => m.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(m => new { m.AuthorId, m.CreatedAt })
                    .IsDescending(false, true)
                    .HasFilter("\"DeletedAt\" IS NULL")
                    .HasDatabaseName("IX_Models_AuthorId_CreatedAt_Active");

                entity.HasIndex(m => m.CreatedAt)
                    .IsDescending(true)
                    .HasFilter($"\"DeletedAt\" IS NULL AND \"Privacy\" = {(int)PrivacySettings.Public}")
                    .HasDatabaseName("IX_Models_CreatedAt_PublicActive");
            });

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.ModelId, l.UserId })
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL")
                .HasDatabaseName("IX_Likes_ModelId_UserId_Active");

            modelBuilder.Entity<ModelViewDedup>(entity =>
            {
                entity.Property(d => d.ViewerKey).HasMaxLength(128);
                entity.HasIndex(d => new { d.ModelId, d.ViewerKey })
                    .IsUnique()
                    .HasDatabaseName("IX_ModelViewDedups_ModelId_ViewerKey");
                entity.HasOne(d => d.Model)
                    .WithMany()
                    .HasForeignKey(d => d.ModelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ModelDownloadDedup>(entity =>
            {
                entity.Property(d => d.ViewerKey).HasMaxLength(128);
                entity.HasIndex(d => new { d.ModelId, d.ViewerKey })
                    .IsUnique()
                    .HasDatabaseName("IX_ModelDownloadDedups_ModelId_ViewerKey");
                entity.HasOne(d => d.Model)
                    .WithMany()
                    .HasForeignKey(d => d.ModelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => new { rt.UserId, rt.CreatedAt })
                    .IsDescending(false, true)
                    .HasFilter("\"RevokedAt\" IS NULL")
                    .HasDatabaseName("IX_RefreshTokens_UserId_CreatedAt_Active");
            });

            modelBuilder.Entity<ExternalAuthProvider>()
                .HasIndex(e => new { e.Provider, e.ExternalId })
                .IsUnique();

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasIndex(t => t.Token).IsUnique();
                entity.HasIndex(t => new { t.Email, t.IsUsed })
                    .HasDatabaseName("IX_PasswordResetTokens_Email_IsUsed");
            });

            modelBuilder.Entity<ReportsDomain.Report>(entity =>
            {
                entity.HasIndex(r => new { r.IsResolved, r.CreatedAt })
                    .IsDescending(false, true)
                    .HasDatabaseName("IX_Reports_IsResolved_CreatedAt");

                entity.HasIndex(r => new { r.Type, r.TargetId, r.CreatedAt })
                    .IsDescending(false, false, true)
                    .HasDatabaseName("IX_Reports_Type_TargetId_CreatedAt");
            });

            modelBuilder.Entity<FederationAuditLog>()
                .HasIndex(a => new { a.FederatedInstanceId, a.EventTimestamp })
                .IsDescending(false, true)
                .HasDatabaseName("IX_FederationAuditLogs_InstanceId_EventTimestamp");

            modelBuilder.Entity<ModerationAuditLog>(entity =>
            {
                entity.HasOne(l => l.PerformedByUser)
                    .WithMany()
                    .HasForeignKey(l => l.PerformedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

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
                .HasOne(h => h.InitiatorInstance)
                .WithMany()
                .HasForeignKey(h => h.InitiatorInstanceId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FederationHandshake>()
                .HasOne(h => h.ResponderInstance)
                .WithMany(i => i.Handshakes)
                .HasForeignKey(h => h.ResponderInstanceId)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<ModelModerationRecord>(entity =>
            {
                entity.ToTable("ModelModeration");
                entity.HasIndex(r => r.ModelId).IsUnique();
                entity.HasOne(r => r.Model)
                    .WithMany()
                    .HasForeignKey(r => r.ModelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EmailMessage>(entity =>
            {
                entity.ToTable("EmailMessages");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.TemplateKey).HasConversion<string>().HasMaxLength(64);
                entity.Property(m => m.Status).HasConversion<string>().HasMaxLength(32);
                entity.Property(m => m.Recipient).HasMaxLength(320).IsRequired();
                entity.Property(m => m.ModelJson).HasColumnType("jsonb").IsRequired();
                entity.Property(m => m.LastError).HasMaxLength(2000);
                entity.Property(m => m.IdempotencyKey).HasMaxLength(200);
                entity.HasIndex(m => new { m.Status, m.NextAttemptAt });
                entity.HasIndex(m => m.CreatedAt);
                entity.HasIndex(m => m.IdempotencyKey)
                    .IsUnique()
                    .HasFilter("\"IdempotencyKey\" IS NOT NULL");
            });

            modelBuilder.Entity<User>()
                .Property(u => u.PendingEmail)
                .HasMaxLength(320);

            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.Property(t => t.Purpose).HasConversion<string>().HasMaxLength(32);
                entity.HasIndex(t => t.Token).IsUnique();
                entity.HasIndex(t => new { t.Email, t.CreatedAt });
            });

            modelBuilder.Entity<UserAuditLog>(entity =>
            {
                entity.ToTable("UserAuditLogs");
                entity.Property(a => a.Action).HasConversion<string>().HasMaxLength(64);
                entity.Property(a => a.Details).HasMaxLength(2000);
                entity.Property(a => a.IpAddress).HasMaxLength(64);
                entity.HasIndex(a => new { a.UserId, a.CreatedAt });
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(a => a.PerformedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CommentDomain.EnhancedComment>(entity =>
            {
                entity.ToTable("EnhancedComments");
                entity.Property(c => c.Content).HasMaxLength(CommentDomain.CommentLimits.MaxContentLength);
                entity.Property(c => c.TargetType).HasConversion<string>().HasMaxLength(32);
                entity.Property(c => c.ModerationReason).HasMaxLength(CommentDomain.CommentLimits.MaxReasonLength);
                entity.HasOne(c => c.Author).WithMany().HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.ModeratedByUser).WithMany().HasForeignKey(c => c.ModeratedByUserId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.ParentComment).WithMany().HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(c => new { c.TargetType, c.TargetId, c.CreatedAt });
                entity.HasIndex(c => new { c.TargetType, c.TargetId, c.CreatedAt })
                    .IsDescending(false, false, true)
                    .HasFilter("\"ParentCommentId\" IS NULL AND NOT \"IsHidden\"")
                    .HasDatabaseName("IX_EnhancedComments_Target_Thread_Active");
                entity.HasIndex(c => c.ParentCommentId);
                entity.HasIndex(c => c.AuthorId);
            });

            modelBuilder.Entity<CommentDomain.CommentReaction>(entity =>
            {
                entity.ToTable("CommentReactions");
                entity.Property(r => r.Type).HasConversion<string>().HasMaxLength(16);
                entity.HasOne(r => r.Comment).WithMany().HasForeignKey(r => r.CommentId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<User>().WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(r => new { r.CommentId, r.UserId }).IsUnique();
                entity.HasIndex(r => r.UserId);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.Property(n => n.Title).HasMaxLength(NotificationLimits.MaxTitleLength).IsRequired();
                entity.Property(n => n.Message).HasMaxLength(NotificationLimits.MaxMessageLength).IsRequired();
                entity.Property(n => n.ActionUrl).HasMaxLength(NotificationLimits.MaxActionUrlLength);
                entity.Property(n => n.DedupeKey).HasMaxLength(NotificationLimits.MaxDedupeKeyLength);
                entity.Property(n => n.RelatedEntityType).HasMaxLength(64);
                entity.Property(n => n.Type).HasConversion<string>().HasMaxLength(32);
                entity.Property(n => n.Priority).HasConversion<string>().HasMaxLength(16);
                entity.HasOne<User>().WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
                entity.HasIndex(n => new { n.UserId, n.DedupeKey });
            });
        }
    }
} 