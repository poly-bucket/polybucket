using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Models.RefreshToken;

namespace Infrastructure.Data.EntityConfigurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Token)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(r => r.Created)
                .IsRequired();
                
            builder.Property(r => r.Expires)
                .IsRequired();
                
            builder.Property(r => r.CreatedByIp)
                .HasMaxLength(50);
                
            builder.Property(r => r.RevokedByIp)
                .HasMaxLength(50)
                .IsRequired(false);
                
            builder.Property(r => r.ReplacedByToken)
                .HasMaxLength(200)
                .IsRequired(false);
                
            builder.Property(r => r.ReasonRevoked)
                .HasMaxLength(200)
                .IsRequired(false);
                
            builder.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 