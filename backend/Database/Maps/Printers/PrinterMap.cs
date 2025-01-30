using Core.Models.Printers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Database.Maps.Printers
{
    public class PrinterMap : IEntityMap<Printer>
    {
        public void Configure(EntityTypeBuilder<Printer> entity)
        {
            entity.ToTable("printers");

            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id")
                .HasColumnType("uuid");

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

            entity.Property(e => e.MaxBedTemp)
                .HasColumnName("max_bed_temp")
                .HasColumnType("int");

            entity.Property(e => e.MaxNozzleTemp)
                .HasColumnName("max_nozzle_temp")
                .HasColumnType("int");

            entity.Property(e => e.HasHeatedBed)
                .HasColumnName("has_heated_bed")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.HasHeatedChamber)
                .HasColumnName("has_heated_chamber")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.HasEnclosure)
                .HasColumnName("has_enclosure")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.DefaultNozzleDiameter)
                .HasColumnName("default_nozzle_diameter")
                .HasColumnType("decimal(4,2)");

            entity.Property(e => e.ExtruderCount)
                .HasColumnName("extruder_count")
                .HasColumnType("int");

            entity.Property(e => e.ExtruderType)
                .HasColumnName("extruder_type")
                .HasColumnType("varchar(50)")
                .HasConversion<string>();

            entity.Property(e => e.Connectivity)
                .HasColumnName("connectivity")
                .HasColumnType("json");

            entity.Property(e => e.SupportedMaterials)
                .HasColumnName("supported_materials")
                .HasColumnType("json");

            entity.Property(e => e.PriceUSD)
                .HasColumnName("price_usd")
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.ReleaseYear)
                .HasColumnName("release_year")
                .HasColumnType("int");

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasColumnType("varchar(50)")
                .HasConversion<string>();

            entity.Property(e => e.HasAutoLeveling)
                .HasColumnName("has_auto_leveling")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.HasFilamentSensor)
                .HasColumnName("has_filament_sensor")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.HasPowerLossContinue)
                .HasColumnName("has_power_loss_continue")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.MaxPrintSpeed)
                .HasColumnName("max_print_speed")
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.WebsiteUrl)
                .HasColumnName("website_url")
                .HasColumnType("varchar(2048)");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp");
        }
    }
}