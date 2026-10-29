using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolyBucket.Database.Entities.Filaments;

namespace PolyBucket.Database.Configurations;

public class FilamentMap : IEntityTypeConfiguration<Filament>
{
    public void Configure(EntityTypeBuilder<Filament> entity)
    {
        entity.ToTable("filaments");

        entity.Property(e => e.Manufacturer)
            .IsRequired()
            .HasColumnName("manufacturer")
            .HasColumnType("varchar(255)");

        entity.Property(e => e.Type)
            .IsRequired()
            .HasColumnName("type")
            .HasColumnType("int");

        entity.Property(e => e.Color)
            .IsRequired()
            .HasColumnName("color")
            .HasColumnType("varchar(50)");

        entity.Property(e => e.Diameter)
            .IsRequired()
            .HasColumnName("diameter")
            .HasColumnType("varchar(10)");
    }
} 