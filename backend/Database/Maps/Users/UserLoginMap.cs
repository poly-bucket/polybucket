using Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Users
{
    public class UserLoginMap : IEntityMap<UserLogin>
    {
        public void Configure(EntityTypeBuilder<UserLogin> entity)
        {
            entity.ToTable("user_logins");

            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id")
                .HasColumnType("uuid");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasColumnName("email")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.IpAddress)
                .IsRequired()
                .HasColumnName("ip_address")
                .HasColumnType("varchar(255)");

            entity.Property(e => e.UserAgent)
                .IsRequired()
                .HasColumnName("user_agent")
                .HasColumnType("varchar(255)");
        }
    }
}