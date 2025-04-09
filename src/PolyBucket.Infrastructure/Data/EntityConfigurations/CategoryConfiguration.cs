using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolyBucket.Core.Entities;

namespace PolyBucket.Infrastructure.Data.EntityConfigurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(c => c.Description)
                .HasMaxLength(5000);
                
            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(c => c.IconUrl)
                .HasMaxLength(500);
            
            // Configure self-referencing relationship for parent/child categories
            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure many-to-many relationship with Model
            builder.HasMany(c => c.Models)
                .WithMany(m => m.Categories)
                .UsingEntity<ModelCategory>(
                    j => j
                        .HasOne(mc => mc.Model)
                        .WithMany(m => m.ModelCategories)
                        .HasForeignKey(mc => mc.ModelId),
                    j => j
                        .HasOne(mc => mc.Category)
                        .WithMany(c => c.ModelCategories)
                        .HasForeignKey(mc => mc.CategoryId),
                    j =>
                    {
                        j.HasKey(mc => new { mc.ModelId, mc.CategoryId });
                        j.ToTable("ModelCategories");
                    }
                );
        }
    }
} 