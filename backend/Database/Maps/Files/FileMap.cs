using Core.Models.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Files
{
    public class FileMap : IEntityMap<Core.Models.Files.File>
    {
        public void Configure(EntityTypeBuilder<Core.Models.Files.File> entity)
        {
            entity.ToTable("files");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Path)
                .IsRequired()
                .HasColumnName("path")
                .HasColumnType("varchar(1024)");

            entity.Property(e => e.Extension)
                .IsRequired()
                .HasColumnName("extension")
                .HasColumnType("varchar(10)");

            entity.Property(e => e.MimeType)
                .IsRequired()
                .HasColumnName("mime_type")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Size)
                .IsRequired()
                .HasColumnName("size")
                .HasColumnType("bigint");

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