using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PolyBucket.Core.Entities;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace PolyBucket.Infrastructure.Data.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(u => u.PasswordHash)
                .IsRequired();
            
            builder.Property(u => u.FirstName)
                .HasMaxLength(50);
            
            builder.Property(u => u.LastName)
                .HasMaxLength(50);
            
            builder.Property(u => u.ProfilePictureUrl)
                .HasMaxLength(255);
            
            builder.Property(u => u.IsAdmin)
                .HasDefaultValue(false);
            
            builder.Property(u => u.IsEmailVerified)
                .HasDefaultValue(false);
            
            builder.Property(u => u.AccessFailedCount)
                .HasDefaultValue(0);
            
            builder.Property(u => u.IsLocked)
                .HasDefaultValue(false);
            
            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            builder.Property(u => u.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            builder.Property(u => u.EmailVerificationToken)
                .HasMaxLength(200);
            
            builder.Property(u => u.PasswordResetToken)
                .HasMaxLength(200);
            
            // Configure Roles list to be stored as JSON with value comparer
            builder.Property(u => u.Roles)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
                
            // Configure OrganizationIds list to be stored as JSON with value comparer
            builder.Property(u => u.OrganizationIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            
            // Configure relationships
            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(u => u.Models)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasMany(u => u.Collections)
                .WithOne()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 