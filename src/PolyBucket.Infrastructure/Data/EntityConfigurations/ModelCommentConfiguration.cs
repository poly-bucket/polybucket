using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolyBucket.Core.Entities;

namespace PolyBucket.Infrastructure.Data.EntityConfigurations
{
    public class ModelCommentConfiguration : IEntityTypeConfiguration<ModelComment>
    {
        public void Configure(EntityTypeBuilder<ModelComment> builder)
        {
            builder.ToTable("ModelComments");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);
                
            builder.Property(c => c.CreatedAt)
                .IsRequired();
                
            // Configure relationships
            builder.HasOne(c => c.Model)
                .WithMany()
                .HasForeignKey(c => c.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 