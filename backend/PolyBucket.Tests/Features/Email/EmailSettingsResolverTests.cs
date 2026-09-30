using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using Xunit;
using Fields = PolyBucket.Api.Common.Email.EffectiveEmailSettings.Fields;

namespace PolyBucket.Tests.Features.Email;

public class EmailSettingsResolverTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly SmtpPasswordProtector _protector;
    private readonly MemoryCache _cache;

    public EmailSettingsResolverTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        _protector = new SmtpPasswordProtector(new EphemeralDataProtectionProvider());
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    private EmailSettingsResolver CreateResolver(EmailOptions? options = null, Dictionary<string, string?>? configuration = null)
    {
        var monitor = new Mock<IOptionsMonitor<EmailOptions>>();
        monitor.Setup(m => m.CurrentValue).Returns(options ?? new EmailOptions());
        var config = new ConfigurationBuilder().AddInMemoryCollection(configuration ?? []).Build();
        return new EmailSettingsResolver(_context, monitor.Object, _protector, _cache, config, NullLogger<EmailSettingsResolver>.Instance);
    }

    private void Seed(params (string Key, string Value)[] settings)
    {
        foreach (var (key, value) in settings)
        {
            _context.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
        }

        _context.SaveChanges();
    }

    [Fact(DisplayName = "When nothing is configured, email delivery is disabled and every field reports its default source.")]
    public async Task GetEffectiveSettings_NothingConfigured_ReturnsDisabledDefaults()
    {
        // Arrange
        var resolver = CreateResolver();

        // Act
        var settings = await resolver.GetEffectiveSettingsAsync();

        // Assert
        settings.Transport.ShouldBe(EmailTransportKind.Disabled);
        settings.IsEnabled.ShouldBeFalse();
        settings.CanDeliver.ShouldBeFalse();
        settings.SmtpPort.ShouldBe(587);
        settings.Sources[Fields.Transport].ShouldBe(EmailSettingSource.Default);
        settings.ManagedFields().ShouldBeEmpty();
    }

    [Fact(DisplayName = "When only database settings exist, the resolver uses them and marks their source as Database.")]
    public async Task GetEffectiveSettings_DatabaseOnly_UsesDatabaseValues()
    {
        // Arrange
        Seed(
            (SystemSettingKeys.EmailTransport, "Smtp"),
            (SystemSettingKeys.EmailSmtpServer, "smtp.db.example"),
            (SystemSettingKeys.EmailSmtpPort, "2525"),
            (SystemSettingKeys.EmailSecurity, "StartTls"),
            (SystemSettingKeys.EmailFromAddress, "db@example.com"));
        var resolver = CreateResolver();

        // Act
        var settings = await resolver.GetEffectiveSettingsAsync();

        // Assert
        settings.Transport.ShouldBe(EmailTransportKind.Smtp);
        settings.SmtpHost.ShouldBe("smtp.db.example");
        settings.SmtpPort.ShouldBe(2525);
        settings.SmtpSecurity.ShouldBe(EmailSecurityMode.StartTls);
        settings.CanDeliver.ShouldBeTrue();
        settings.Sources[Fields.SmtpHost].ShouldBe(EmailSettingSource.Database);
        settings.IsManagedByEnvironment(Fields.SmtpHost).ShouldBeFalse();
    }

    [Fact(DisplayName = "When environment options and database settings both exist, environment values win and are marked as managed.")]
    public async Task GetEffectiveSettings_EnvironmentAndDatabase_EnvironmentWins()
    {
        // Arrange
        Seed(
            (SystemSettingKeys.EmailSmtpServer, "smtp.db.example"),
            (SystemSettingKeys.EmailFromAddress, "db@example.com"));
        var options = new EmailOptions
        {
            FromAddress = "env@example.com",
            Smtp = new SmtpOptions { Host = "smtp.env.example", Port = 465 }
        };
        var resolver = CreateResolver(options);

        // Act
        var settings = await resolver.GetEffectiveSettingsAsync();

        // Assert
        settings.Transport.ShouldBe(EmailTransportKind.Smtp);
        settings.SmtpHost.ShouldBe("smtp.env.example");
        settings.SmtpPort.ShouldBe(465);
        settings.FromAddress.ShouldBe("env@example.com");
        settings.IsManagedByEnvironment(Fields.Transport).ShouldBeTrue();
        settings.IsManagedByEnvironment(Fields.SmtpHost).ShouldBeTrue();
        settings.IsManagedByEnvironment(Fields.FromAddress).ShouldBeTrue();
    }

    [Fact(DisplayName = "When only the legacy Email:Enabled and UseSsl settings exist on port 465, the resolver maps them to SMTP with SSL on connect.")]
    public async Task GetEffectiveSettings_LegacySettings_AreMapped()
    {
        // Arrange
        Seed(
            (SystemSettingKeys.EmailEnabled, "true"),
            (SystemSettingKeys.EmailUseSsl, "true"),
            (SystemSettingKeys.EmailSmtpPort, "465"),
            (SystemSettingKeys.EmailSmtpServer, "smtp.legacy.example"),
            (SystemSettingKeys.EmailFromAddress, "legacy@example.com"));
        var resolver = CreateResolver();

        // Act
        var settings = await resolver.GetEffectiveSettingsAsync();

        // Assert
        settings.Transport.ShouldBe(EmailTransportKind.Smtp);
        settings.SmtpSecurity.ShouldBe(EmailSecurityMode.SslOnConnect);
    }

    [Fact(DisplayName = "When the stored SMTP password is legacy plaintext, the resolver returns it and re-saves it encrypted.")]
    public async Task GetEffectiveSettings_LegacyPlaintextPassword_IsUpgraded()
    {
        // Arrange
        Seed((SystemSettingKeys.EmailSmtpPassword, "hunter2"));
        var resolver = CreateResolver();

        // Act
        var settings = await resolver.GetEffectiveSettingsAsync();

        // Assert
        settings.SmtpPassword.ShouldBe("hunter2");
        var stored = await _context.SystemSettings.SingleAsync(s => s.Key == SystemSettingKeys.EmailSmtpPassword);
        stored.Value.ShouldStartWith(SmtpPasswordProtector.Prefix);
        stored.Value.ShouldNotContain("hunter2");
    }

    [Fact(DisplayName = "When the SMTP password is supplied through a password file, the resolver reads it and trims the trailing newline.")]
    public async Task GetEffectiveSettings_PasswordFile_IsRead()
    {
        // Arrange
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "file-secret\n");
        var resolver = CreateResolver(new EmailOptions { Smtp = new SmtpOptions { PasswordFile = path } });

        try
        {
            // Act
            var settings = await resolver.GetEffectiveSettingsAsync();

            // Assert
            settings.SmtpPassword.ShouldBe("file-secret");
            settings.IsManagedByEnvironment(Fields.SmtpPassword).ShouldBeTrue();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact(DisplayName = "When no public base URL is set anywhere, the resolver falls back to AppSettings:Frontend:BaseUrl without locking the field.")]
    public async Task GetEffectiveSettings_FrontendBaseUrl_IsDefaultFallback()
    {
        // Arrange
        var resolver = CreateResolver(configuration: new Dictionary<string, string?> { ["AppSettings:Frontend:BaseUrl"] = "https://models.example.com" });

        // Act
        var settings = await resolver.GetEffectiveSettingsAsync();

        // Assert
        settings.PublicBaseUrl.ShouldBe("https://models.example.com");
        settings.Sources[Fields.PublicBaseUrl].ShouldBe(EmailSettingSource.Default);
    }

    [Fact(DisplayName = "When settings are cached, a database change is only visible after Invalidate is called.")]
    public async Task GetEffectiveSettings_UsesCacheUntilInvalidated()
    {
        // Arrange
        Seed((SystemSettingKeys.EmailFromAddress, "first@example.com"));
        var resolver = CreateResolver();
        await resolver.GetEffectiveSettingsAsync();
        var setting = await _context.SystemSettings.SingleAsync(s => s.Key == SystemSettingKeys.EmailFromAddress);
        setting.Value = "second@example.com";
        await _context.SaveChangesAsync();

        // Act
        var cached = await resolver.GetEffectiveSettingsAsync();
        resolver.Invalidate();
        var refreshed = await resolver.GetEffectiveSettingsAsync();

        // Assert
        cached.FromAddress.ShouldBe("first@example.com");
        refreshed.FromAddress.ShouldBe("second@example.com");
    }

    public void Dispose()
    {
        _context.Dispose();
        _cache.Dispose();
    }
}

internal static class EffectiveEmailSettingsTestExtensions
{
    public static IEnumerable<string> ManagedFields(this EffectiveEmailSettings settings) =>
        Fields.All.Where(settings.IsManagedByEnvironment);
}
