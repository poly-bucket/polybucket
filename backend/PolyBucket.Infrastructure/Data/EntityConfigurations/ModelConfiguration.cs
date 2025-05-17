using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;
using Core.Models.Models;

namespace Infrastructure.Data.EntityConfigurations
{
    public class ModelConfiguration : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder.HasKey(m => m.Id);
            
            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(m => m.Description)
                .HasMaxLength(5000);
                
            builder.Property(m => m.ThumbnailUrl)
                .HasMaxLength(500);
                
            builder.Property(m => m.License)
                .HasMaxLength(100);
                
            builder.Property(m => m.VersionLabel)
                .HasMaxLength(100);
                
            builder.Property(m => m.ModerationReason)
                .HasMaxLength(1000);
            
            // Configure relationships
            builder.HasOne(m => m.User)
                .WithMany(u => u.Models)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(m => m.Collection)
                .WithMany(c => c.Models)
                .HasForeignKey(m => m.CollectionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            builder.HasOne(m => m.ParentVersion)
                .WithMany(m => m.ChildVersions)
                .HasForeignKey(m => m.ParentVersionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure many-to-many relationships
            builder.HasMany(m => m.Tags)
                .WithMany(t => t.Models)
                .UsingEntity<ModelTag>(
                    j => j
                        .HasOne(mt => mt.Tag)
                        .WithMany()
                        .HasForeignKey(mt => mt.TagId),
                    j => j
                        .HasOne(mt => mt.Model)
                        .WithMany()
                        .HasForeignKey(mt => mt.ModelId),
                    j =>
                    {
                        j.HasKey(mt => new { mt.ModelId, mt.TagId });
                        j.ToTable("ModelTags");
                    }
                );
                
            builder.HasMany(m => m.Categories)
                .WithMany(c => c.Models)
                .UsingEntity<ModelCategory>(
                    j => j
                        .HasOne(mc => mc.Category)
                        .WithMany()
                        .HasForeignKey(mc => mc.CategoryId),
                    j => j
                        .HasOne(mc => mc.Model)
                        .WithMany()
                        .HasForeignKey(mc => mc.ModelId),
                    j =>
                    {
                        j.HasKey(mc => new { mc.ModelId, mc.CategoryId });
                        j.ToTable("ModelCategories");
                    }
                );
        }
    }
} 