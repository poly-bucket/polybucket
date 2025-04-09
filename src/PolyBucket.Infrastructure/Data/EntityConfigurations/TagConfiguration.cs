using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolyBucket.Core.Entities;

namespace PolyBucket.Infrastructure.Data.EntityConfigurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tag");
            
            builder.HasKey(t => t.Id);
            
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(t => t.CreatedAt)
                .IsRequired();
                
            // Add a unique index on Name and Slug
            builder.HasIndex(t => t.Name).IsUnique();
            builder.HasIndex(t => t.Slug).IsUnique();
            
            // Configure many-to-many relationship with Model
            builder.HasMany(t => t.Models)
                .WithMany(m => m.Tags)
                .UsingEntity<ModelTag>(
                    j => j
                        .HasOne(mt => mt.Model)
                        .WithMany(m => m.ModelTags)
                        .HasForeignKey(mt => mt.ModelId),
                    j => j
                        .HasOne(mt => mt.Tag)
                        .WithMany(t => t.ModelTags)
                        .HasForeignKey(mt => mt.TagId),
                    j =>
                    {
                        j.HasKey(t => t.Id);
                        j.ToTable("ModelTag");
                    });
        }
    }
} 