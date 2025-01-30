using Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Users
{
    public class UserMap : IEntityMap<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("users");

            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id")
                .HasColumnType("uuid");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasColumnName("email")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Username)
                .IsRequired()
                .HasColumnName("username")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.FirstName)
                .IsRequired(false)
                .HasColumnName("first_name")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.LastName)
                .IsRequired(false)
                .HasColumnName("last_name")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Salt)
                .IsRequired()
                .HasColumnName("salt")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasColumnName("password_hash")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Country)
                .IsRequired(false)
                .HasColumnName("country")
                .HasColumnType("varchar(255)");

            entity.Property(x => x.Role)
                .HasColumnType("varchar(20)")
                .HasConversion<string>();
        }
    }
}