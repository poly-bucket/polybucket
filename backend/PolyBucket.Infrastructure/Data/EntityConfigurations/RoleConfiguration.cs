using Core.Models.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EntityConfigurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(r => r.Description)
                .HasMaxLength(255);
            
            builder.Property(r => r.IsSystemRole)
                .HasDefaultValue(false);
            
            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            builder.Property(r => r.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Add a unique index on Name
            builder.HasIndex(r => r.Name)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL"); // Only apply uniqueness to non-deleted roles
            
            // Add the soft delete filter at the query level
            builder.HasQueryFilter(r => r.DeletedAt == null);
        }
    }
}