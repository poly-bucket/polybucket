using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps.Users;

public class UserSettingsMap : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UserId).IsRequired();
        builder.HasOne(x => x.User)
            .WithOne(x => x.Settings)
            .HasForeignKey<UserSettings>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Language).IsRequired();
        builder.Property(x => x.Theme).IsRequired();
        builder.Property(x => x.EmailNotifications).IsRequired();
        builder.Property(x => x.PushNotifications).IsRequired();
    }
} 