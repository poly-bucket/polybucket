using Core.Models.Filaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Filaments
{
    public class FilamentMap : IEntityMap<Filament>
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
} 