using Core.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Models
{
    public class ModelMap : IEntityMap<Model>
    {
        public void Configure(EntityTypeBuilder<Model> entity)
        {
            entity.ToTable("models");

            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id")
                .HasColumnType("uuid");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            entity.Property(e => e.License)
                .IsRequired(false)
                .HasColumnName("license")
                .HasColumnType("int");

            entity.Property(e => e.Privacy)
                .IsRequired()
                .HasColumnName("privacy")
                .HasColumnType("int");

            entity.Property(e => e.Categories)
                .IsRequired()
                .HasColumnName("categories")
                .HasColumnType("json");

            entity.Property(e => e.AIGenerated)
                .IsRequired()
                .HasColumnName("ai_generated")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.WIP)
                .IsRequired()
                .HasColumnName("wip")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.NSFW)
                .IsRequired()
                .HasColumnName("nsfw")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.IsRemix)
                .HasColumnName("is_remix")
                .HasColumnType("varchar(255)");

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

            // Navigation properties
            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey("created_by_id");

            entity.HasMany(e => e.Files)
                .WithOne()
                .HasForeignKey("model_id");
        }
    }
}