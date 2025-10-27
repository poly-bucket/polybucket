using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Data
{
    public class MarketplaceDbContext : DbContext
    {
        public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options)
        {
        }

        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<PluginVersion> PluginVersions { get; set; }
        public DbSet<PluginCategory> Categories { get; set; }
        public DbSet<PluginReview> Reviews { get; set; }
        public DbSet<PluginReview> PluginReviews { get; set; }
        public DbSet<ReviewHelpful> ReviewHelpfuls { get; set; }
        public DbSet<PluginRating> Ratings { get; set; }
        public DbSet<PluginDownload> Downloads { get; set; }
        public DbSet<PluginSubmission> Submissions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Plugin configuration
            modelBuilder.Entity<Plugin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.LongDescription).HasMaxLength(10000);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RepositoryUrl).HasMaxLength(500);
                entity.Property(e => e.License).HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Downloads).HasDefaultValue(0);
                entity.Property(e => e.AverageRating).HasDefaultValue(0.0);
                entity.Property(e => e.Revenue).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsVerified);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.AuthorId);
            });

            // Plugin Version configuration
            modelBuilder.Entity<PluginVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DownloadUrl).HasMaxLength(500);
                entity.Property(e => e.ReleaseNotes).HasMaxLength(5000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Plugin)
                      .WithMany(p => p.Versions)
                      .HasForeignKey(e => e.PluginId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PluginId, e.Version }).IsUnique();
            });

            // Category configuration
            modelBuilder.Entity<PluginCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Review configuration
            modelBuilder.Entity<PluginReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Title).HasMaxLength(255);
                entity.Property(e => e.Content).HasMaxLength(2000);
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.HelpfulCount).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Plugin)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(e => e.PluginId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PluginId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.PluginId, e.UserId }).IsUnique();
            });

            // Review Helpful configuration
            modelBuilder.Entity<ReviewHelpful>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Review)
                      .WithMany(r => r.Helpfuls)
                      .HasForeignKey(e => e.ReviewId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
            });

            // Rating configuration
            modelBuilder.Entity<PluginRating>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Plugin)
                      .WithMany()
                      .HasForeignKey(e => e.PluginId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.PluginId, e.UserId }).IsUnique();
            });

            // Download configuration
            modelBuilder.Entity<PluginDownload>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DownloadedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Plugin)
                      .WithMany()
                      .HasForeignKey(e => e.PluginId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PluginId);
                entity.HasIndex(e => e.DownloadedAt);
            });

            // Submission configuration
            modelBuilder.Entity<PluginSubmission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RepositoryUrl).HasMaxLength(500);
                entity.Property(e => e.License).HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SubmittedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.ReviewedAt);
                entity.Property(e => e.PublishedAt);

                entity.HasOne(e => e.Submitter)
                      .WithMany()
                      .HasForeignKey(e => e.SubmitterId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.SubmittedAt);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.AvatarUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}
