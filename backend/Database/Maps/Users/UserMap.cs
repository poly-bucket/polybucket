using Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Users
{
    public class UserMap : IEntityMap<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.Property(e => e.Email)
                .IsRequired()
                .HasColumnName("email")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.Username)
                .IsRequired()
                .HasColumnName("username")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.FirstName)
                .HasColumnName("first_name")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.LastName)
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
        }
    }
}