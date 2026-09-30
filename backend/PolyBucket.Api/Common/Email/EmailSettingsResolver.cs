using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Common.Email;

public interface IEmailSettingsResolver
{
    Task<EffectiveEmailSettings> GetEffectiveSettingsAsync(CancellationToken cancellationToken = default);
    void Invalidate();
}

public class EmailSettingsResolver(
    PolyBucketDbContext context,
    IOptionsMonitor<EmailOptions> options,
    ISmtpPasswordProtector passwordProtector,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<EmailSettingsResolver> logger) : IEmailSettingsResolver
{
    public const string CacheKey = "email:effective-settings";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<EffectiveEmailSettings> GetEffectiveSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out EffectiveEmailSettings? cached) && cached != null)
        {
            return cached;
        }

        var dbSettings = await context.SystemSettings
            .Where(s => s.Key.StartsWith("Email:"))
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        var settings = await BuildAsync(options.CurrentValue, dbSettings, cancellationToken);
        cache.Set(CacheKey, settings, CacheDuration);
        return settings;
    }

    public void Invalidate()
    {
        cache.Remove(CacheKey);
    }

    private async Task<EffectiveEmailSettings> BuildAsync(
        EmailOptions env,
        IReadOnlyDictionary<string, string> db,
        CancellationToken cancellationToken)
    {
        var sources = new Dictionary<string, EmailSettingSource>();

        var transport = ResolveTransport(env, db, sources);

        var host = Pick(EffectiveEmailSettings.Fields.SmtpHost, NullIfBlank(env.Smtp.Host), Get(db, SystemSettingKeys.EmailSmtpServer), string.Empty, sources);
        var port = Pick(EffectiveEmailSettings.Fields.SmtpPort, env.Smtp.Port, ParseInt(Get(db, SystemSettingKeys.EmailSmtpPort)), 587, sources);
        var security = ResolveSecurity(env, db, port, sources);
        var username = Pick(EffectiveEmailSettings.Fields.SmtpUsername, NullIfBlank(env.Smtp.Username), Get(db, SystemSettingKeys.EmailSmtpUsername), string.Empty, sources);
        var password = await ResolvePasswordAsync(env, db, sources, cancellationToken);
        var allowInvalid = Pick(EffectiveEmailSettings.Fields.AllowInvalidCertificates, env.Smtp.AllowInvalidCertificates, ParseBool(Get(db, SystemSettingKeys.EmailAllowInvalidCertificates)), false, sources);
        var fromAddress = Pick(EffectiveEmailSettings.Fields.FromAddress, NullIfBlank(env.FromAddress), Get(db, SystemSettingKeys.EmailFromAddress), string.Empty, sources);
        var fromName = Pick(EffectiveEmailSettings.Fields.FromName, NullIfBlank(env.FromName), Get(db, SystemSettingKeys.EmailFromName), "PolyBucket", sources);
        var replyTo = Pick<string?>(EffectiveEmailSettings.Fields.ReplyTo, NullIfBlank(env.ReplyTo), Get(db, SystemSettingKeys.EmailReplyTo), null, sources);
        var publicBaseUrl = Pick(
            EffectiveEmailSettings.Fields.PublicBaseUrl,
            NullIfBlank(env.PublicBaseUrl),
            Get(db, SystemSettingKeys.EmailPublicBaseUrl),
            NullIfBlank(configuration["AppSettings:Frontend:BaseUrl"]) ?? string.Empty,
            sources);
        var requireVerification = Pick(EffectiveEmailSettings.Fields.RequireEmailVerification, env.RequireEmailVerification, ParseBool(Get(db, SystemSettingKeys.EmailRequireVerification)), false, sources);

        DateTime? lastTest = DateTime.TryParse(
            Get(db, SystemSettingKeys.EmailLastSuccessfulTestAt),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
            out var parsedLastTest)
            ? parsedLastTest
            : null;

        return new EffectiveEmailSettings
        {
            Transport = transport,
            SmtpHost = host,
            SmtpPort = port,
            SmtpSecurity = security,
            SmtpUsername = username,
            SmtpPassword = password,
            TimeoutSeconds = env.Smtp.TimeoutSeconds,
            AllowInvalidCertificates = allowInvalid,
            FromAddress = fromAddress,
            FromName = fromName,
            ReplyTo = replyTo,
            PublicBaseUrl = publicBaseUrl,
            RequireEmailVerification = requireVerification,
            LastSuccessfulTestAt = lastTest,
            Sources = sources
        };
    }

    private static EmailTransportKind ResolveTransport(EmailOptions env, IReadOnlyDictionary<string, string> db, Dictionary<string, EmailSettingSource> sources)
    {
        const string field = EffectiveEmailSettings.Fields.Transport;
        if (env.Transport.HasValue)
        {
            sources[field] = EmailSettingSource.Environment;
            return env.Transport.Value;
        }

        if (!string.IsNullOrWhiteSpace(env.Smtp.Host))
        {
            sources[field] = EmailSettingSource.Environment;
            return EmailTransportKind.Smtp;
        }

        if (Enum.TryParse<EmailTransportKind>(Get(db, SystemSettingKeys.EmailTransport), true, out var stored))
        {
            sources[field] = EmailSettingSource.Database;
            return stored;
        }

        if (ParseBool(Get(db, SystemSettingKeys.EmailEnabled)) == true)
        {
            sources[field] = EmailSettingSource.Database;
            return EmailTransportKind.Smtp;
        }

        sources[field] = EmailSettingSource.Default;
        return EmailTransportKind.Disabled;
    }

    private static EmailSecurityMode ResolveSecurity(EmailOptions env, IReadOnlyDictionary<string, string> db, int port, Dictionary<string, EmailSettingSource> sources)
    {
        const string field = EffectiveEmailSettings.Fields.SmtpSecurity;
        if (env.Smtp.Security.HasValue)
        {
            sources[field] = EmailSettingSource.Environment;
            return env.Smtp.Security.Value;
        }

        if (Enum.TryParse<EmailSecurityMode>(Get(db, SystemSettingKeys.EmailSecurity), true, out var stored))
        {
            sources[field] = EmailSettingSource.Database;
            return stored;
        }

        var legacyUseSsl = ParseBool(Get(db, SystemSettingKeys.EmailUseSsl));
        if (legacyUseSsl.HasValue)
        {
            sources[field] = EmailSettingSource.Database;
            return legacyUseSsl.Value && port == 465 ? EmailSecurityMode.SslOnConnect : EmailSecurityMode.Auto;
        }

        sources[field] = EmailSettingSource.Default;
        return EmailSecurityMode.Auto;
    }

    private async Task<string> ResolvePasswordAsync(
        EmailOptions env,
        IReadOnlyDictionary<string, string> db,
        Dictionary<string, EmailSettingSource> sources,
        CancellationToken cancellationToken)
    {
        const string field = EffectiveEmailSettings.Fields.SmtpPassword;
        if (!string.IsNullOrEmpty(env.Smtp.Password))
        {
            sources[field] = EmailSettingSource.Environment;
            return env.Smtp.Password;
        }

        if (!string.IsNullOrWhiteSpace(env.Smtp.PasswordFile))
        {
            sources[field] = EmailSettingSource.Environment;
            try
            {
                return (await File.ReadAllTextAsync(env.Smtp.PasswordFile, cancellationToken)).TrimEnd('\r', '\n');
            }
            catch (IOException ex)
            {
                logger.LogError(ex, "Unable to read SMTP password file configured in Email:Smtp:PasswordFile");
                return string.Empty;
            }
        }

        var stored = Get(db, SystemSettingKeys.EmailSmtpPassword);
        if (string.IsNullOrEmpty(stored))
        {
            sources[field] = EmailSettingSource.Default;
            return string.Empty;
        }

        sources[field] = EmailSettingSource.Database;
        var result = passwordProtector.Unprotect(stored);
        if (result.IsUnreadable)
        {
            logger.LogError("The stored SMTP password could not be decrypted. Data Protection keys may have changed; re-enter the password in the admin email settings.");
            return string.Empty;
        }

        if (result.IsLegacyPlaintext)
        {
            await UpgradeLegacyPasswordAsync(result.Plaintext, cancellationToken);
        }

        return result.Plaintext;
    }

    private async Task UpgradeLegacyPasswordAsync(string plaintext, CancellationToken cancellationToken)
    {
        try
        {
            var setting = await context.SystemSettings.FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.EmailSmtpPassword, cancellationToken);
            if (setting == null)
            {
                return;
            }

            setting.Value = passwordProtector.Protect(plaintext);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Upgraded stored SMTP password to encrypted storage");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to upgrade legacy SMTP password to encrypted storage");
        }
    }

    private static T Pick<T>(string field, T? environmentValue, T? databaseValue, T defaultValue, Dictionary<string, EmailSettingSource> sources)
    {
        if (environmentValue is not null)
        {
            sources[field] = EmailSettingSource.Environment;
            return environmentValue;
        }

        if (databaseValue is not null)
        {
            sources[field] = EmailSettingSource.Database;
            return databaseValue;
        }

        sources[field] = EmailSettingSource.Default;
        return defaultValue;
    }

    private static int Pick(string field, int? environmentValue, int? databaseValue, int defaultValue, Dictionary<string, EmailSettingSource> sources)
    {
        return Pick<int?>(field, environmentValue, databaseValue, defaultValue, sources) ?? defaultValue;
    }

    private static bool Pick(string field, bool? environmentValue, bool? databaseValue, bool defaultValue, Dictionary<string, EmailSettingSource> sources)
    {
        return Pick<bool?>(field, environmentValue, databaseValue, defaultValue, sources) ?? defaultValue;
    }

    private static string? Get(IReadOnlyDictionary<string, string> db, string key)
    {
        return db.TryGetValue(key, out var value) ? NullIfBlank(value) : null;
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static int? ParseInt(string? value) => int.TryParse(value, out var parsed) ? parsed : null;

    private static bool? ParseBool(string? value) => bool.TryParse(value, out var parsed) ? parsed : null;
}
