using Core.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Models
{
    public class PrintSettingsMap : IEntityMap<PrintSettings>
    {
        public void Configure(EntityTypeBuilder<PrintSettings> entity)
        {
            entity.ToTable("print_settings");

            entity.HasKey(e => e.Id)
                .HasName("Id");

            entity.Property(e => e.Supports)
                .IsRequired()
                .HasColumnName("supports")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.LayerHeight)
                .IsRequired()
                .HasColumnName("layer_height")
                .HasColumnType("decimal(5,3)");

            entity.Property(e => e.WallLoops)
                .IsRequired()
                .HasColumnName("wall_loops")
                .HasColumnType("int");

            entity.Property(e => e.InfillPercentage)
                .IsRequired()
                .HasColumnName("infill_percentage")
                .HasColumnType("int");
        }
    }
} 