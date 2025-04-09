using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolyBucket.Core.Models;

namespace PolyBucket.Infrastructure.Data.EntityConfigurations
{
    public class ModelsModelCommentConfiguration : IEntityTypeConfiguration<ModelComment>
    {
        public void Configure(EntityTypeBuilder<ModelComment> builder)
        {
            builder.ToTable("ModelsModelComments");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);
                
            builder.Property(c => c.CreatedAt)
                .IsRequired();
                
            // Configure relationships
            builder.HasOne(c => c.Model)
                .WithMany(m => m.Comments)
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