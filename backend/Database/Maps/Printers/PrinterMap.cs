using Core.Models.Printers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Printers
{
    public class PrinterMap : IEntityMap<Printer>
    {
        public void Configure(EntityTypeBuilder<Printer> entity)
        {
            entity.ToTable("printers");

            entity.Property(e => e.Manufacturer)
                .IsRequired()
                .HasColumnName("manufacturer")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Model)
                .IsRequired()
                .HasColumnName("model")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.BuildVolumeX)
                .IsRequired()
                .HasColumnName("build_volume_x")
                .HasColumnType("int");

            entity.Property(e => e.BuildVolumeY)
                .IsRequired()
                .HasColumnName("build_volume_y")
                .HasColumnType("int");

            entity.Property(e => e.BuildVolumeZ)
                .IsRequired()
                .HasColumnName("build_volume_z")
                .HasColumnType("int");
        }
    }
} 