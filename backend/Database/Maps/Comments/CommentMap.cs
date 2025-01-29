using Core.Models.Comments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Comments
{
    public class CommentMap : IEntityMap<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> entity)
        {
            entity.ToTable("comments");

            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnName("content")
                .HasColumnType("text");

            entity.Property(e => e.Likes)
                .IsRequired()
                .HasColumnName("likes")
                .HasColumnType("int")
                .HasDefaultValue(0);

            entity.Property(e => e.Dislikes)
                .IsRequired()
                .HasColumnName("dislikes")
                .HasColumnType("int")
                .HasDefaultValue(0);

            // Navigation properties
            entity.HasOne(e => e.Model)
                .WithMany()
                .HasForeignKey("model_id")
                .IsRequired();

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey("author_id")
                .IsRequired();

            // Audit fields
            entity.Property(e => e.CreatedById)
                .IsRequired()
                .HasColumnName("created_by_id");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedById)
                .IsRequired()
                .HasColumnName("updated_by_id");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");

            entity.Property(e => e.DeletedById)
                .HasColumnName("deleted_by_id");

            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
        }
    }
} 