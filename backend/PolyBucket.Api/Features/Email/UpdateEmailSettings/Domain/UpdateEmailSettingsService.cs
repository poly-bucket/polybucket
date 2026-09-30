using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Fields = PolyBucket.Api.Common.Email.EffectiveEmailSettings.Fields;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Domain;

public class UpdateEmailSettingsService(
    IUpdateEmailSettingsRepository repository,
    IEmailSettingsResolver settingsResolver,
    ISmtpPasswordProtector passwordProtector,
    TimeProvider timeProvider,
    ILogger<UpdateEmailSettingsService> logger) : IUpdateEmailSettingsService
{
    public static readonly TimeSpan VerificationGuardrailWindow = TimeSpan.FromHours(24);

    public async Task<EmailSettingsDto> UpdateAsync(EmailSettingsUpdate update, CancellationToken cancellationToken = default)
    {
        var current = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        var normalized = Normalize(update);

        var conflicts = FindEnvironmentConflicts(current, normalized);
        if (conflicts.Count > 0)
        {
            throw new ConflictException(
                $"These settings are managed by environment variables and cannot be changed here: {string.Join(", ", conflicts)}.");
        }

        var passwordAfterUpdate = normalized.ClearPassword
            ? string.Empty
            : !string.IsNullOrEmpty(normalized.SmtpPassword) ? normalized.SmtpPassword : current.SmtpPassword;

        var candidate = Merge(current, normalized, passwordAfterUpdate);
        var errors = new List<string>(candidate.GetValidationErrors());

        if (candidate.RequireEmailVerification && !candidate.IsEnabled)
        {
            errors.Add("Email verification cannot be required while email delivery is disabled.");
        }

        var connectionChanged = HasConnectionChanged(current, candidate);
        if (candidate.RequireEmailVerification && !current.RequireEmailVerification)
        {
            var recentTest = current.LastSuccessfulTestAt.HasValue
                && timeProvider.GetUtcNow().UtcDateTime - current.LastSuccessfulTestAt.Value <= VerificationGuardrailWindow;
            if (!recentTest || connectionChanged)
            {
                errors.Add("Send a successful test email with the current settings before requiring email verification.");
            }
        }

        if (candidate.RequireEmailVerification && !candidate.HasPublicBaseUrl)
        {
            errors.Add("A public base URL is required so verification links point to this site.");
        }

        if (errors.Count > 0)
        {
            throw new DomainValidationException(errors);
        }

        var values = BuildValues(current, normalized, connectionChanged);
        await repository.UpsertAsync(values, cancellationToken);
        settingsResolver.Invalidate();

        logger.LogInformation("Email settings updated (transport {Transport}, connection changed: {ConnectionChanged})", candidate.Transport, connectionChanged);

        return EmailSettingsDto.From(await settingsResolver.GetEffectiveSettingsAsync(cancellationToken));
    }

    private static EmailSettingsUpdate Normalize(EmailSettingsUpdate update)
    {
        return update with
        {
            SmtpHost = update.SmtpHost?.Trim() ?? string.Empty,
            SmtpUsername = update.SmtpUsername?.Trim() ?? string.Empty,
            FromAddress = update.FromAddress?.Trim() ?? string.Empty,
            FromName = string.IsNullOrWhiteSpace(update.FromName) ? "PolyBucket" : update.FromName.Trim(),
            ReplyTo = string.IsNullOrWhiteSpace(update.ReplyTo) ? null : update.ReplyTo.Trim(),
            PublicBaseUrl = update.PublicBaseUrl?.Trim().TrimEnd('/') ?? string.Empty
        };
    }

    private static List<string> FindEnvironmentConflicts(EffectiveEmailSettings current, EmailSettingsUpdate update)
    {
        var conflicts = new List<string>();

        void Check(string field, bool differs)
        {
            if (differs && current.IsManagedByEnvironment(field))
            {
                conflicts.Add(field);
            }
        }

        Check(Fields.Transport, update.Transport != current.Transport);
        Check(Fields.SmtpHost, !string.Equals(update.SmtpHost, current.SmtpHost, StringComparison.OrdinalIgnoreCase));
        Check(Fields.SmtpPort, update.SmtpPort != current.SmtpPort);
        Check(Fields.SmtpSecurity, update.SmtpSecurity != current.SmtpSecurity);
        Check(Fields.SmtpUsername, !string.Equals(update.SmtpUsername, current.SmtpUsername, StringComparison.Ordinal));
        Check(Fields.SmtpPassword, update.ClearPassword || !string.IsNullOrEmpty(update.SmtpPassword));
        Check(Fields.AllowInvalidCertificates, update.AllowInvalidCertificates != current.AllowInvalidCertificates);
        Check(Fields.FromAddress, !string.Equals(update.FromAddress, current.FromAddress, StringComparison.OrdinalIgnoreCase));
        Check(Fields.FromName, !string.Equals(update.FromName, current.FromName, StringComparison.Ordinal));
        Check(Fields.ReplyTo, !string.Equals(update.ReplyTo ?? string.Empty, current.ReplyTo ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        Check(Fields.PublicBaseUrl, !string.Equals(update.PublicBaseUrl, current.PublicBaseUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
        Check(Fields.RequireEmailVerification, update.RequireEmailVerification != current.RequireEmailVerification);

        return conflicts;
    }

    private static EffectiveEmailSettings Merge(EffectiveEmailSettings current, EmailSettingsUpdate update, string password)
    {
        return current with
        {
            Transport = update.Transport,
            SmtpHost = update.SmtpHost,
            SmtpPort = update.SmtpPort,
            SmtpSecurity = update.SmtpSecurity,
            SmtpUsername = update.SmtpUsername,
            SmtpPassword = password,
            AllowInvalidCertificates = update.AllowInvalidCertificates,
            FromAddress = update.FromAddress,
            FromName = update.FromName,
            ReplyTo = update.ReplyTo,
            PublicBaseUrl = string.IsNullOrEmpty(update.PublicBaseUrl) ? current.PublicBaseUrl : update.PublicBaseUrl,
            RequireEmailVerification = update.RequireEmailVerification
        };
    }

    private static bool HasConnectionChanged(EffectiveEmailSettings current, EffectiveEmailSettings candidate)
    {
        return current.Transport != candidate.Transport
            || !string.Equals(current.SmtpHost, candidate.SmtpHost, StringComparison.OrdinalIgnoreCase)
            || current.SmtpPort != candidate.SmtpPort
            || current.SmtpSecurity != candidate.SmtpSecurity
            || !string.Equals(current.SmtpUsername, candidate.SmtpUsername, StringComparison.Ordinal)
            || !string.Equals(current.SmtpPassword, candidate.SmtpPassword, StringComparison.Ordinal)
            || current.AllowInvalidCertificates != candidate.AllowInvalidCertificates
            || !string.Equals(current.FromAddress, candidate.FromAddress, StringComparison.OrdinalIgnoreCase);
    }

    private Dictionary<string, string> BuildValues(EffectiveEmailSettings current, EmailSettingsUpdate update, bool connectionChanged)
    {
        var values = new Dictionary<string, string>();

        void Set(string field, string key, string value)
        {
            if (!current.IsManagedByEnvironment(field))
            {
                values[key] = value;
            }
        }

        Set(Fields.Transport, SystemSettingKeys.EmailTransport, update.Transport.ToString());
        Set(Fields.Transport, SystemSettingKeys.EmailEnabled, (update.Transport != EmailTransportKind.Disabled).ToString().ToLowerInvariant());
        Set(Fields.SmtpHost, SystemSettingKeys.EmailSmtpServer, update.SmtpHost);
        Set(Fields.SmtpPort, SystemSettingKeys.EmailSmtpPort, update.SmtpPort.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Set(Fields.SmtpSecurity, SystemSettingKeys.EmailSecurity, update.SmtpSecurity.ToString());
        Set(Fields.SmtpSecurity, SystemSettingKeys.EmailUseSsl, (update.SmtpSecurity == EmailSecurityMode.SslOnConnect).ToString().ToLowerInvariant());
        Set(Fields.SmtpUsername, SystemSettingKeys.EmailSmtpUsername, update.SmtpUsername);
        Set(Fields.AllowInvalidCertificates, SystemSettingKeys.EmailAllowInvalidCertificates, update.AllowInvalidCertificates.ToString().ToLowerInvariant());
        Set(Fields.FromAddress, SystemSettingKeys.EmailFromAddress, update.FromAddress);
        Set(Fields.FromName, SystemSettingKeys.EmailFromName, update.FromName);
        Set(Fields.ReplyTo, SystemSettingKeys.EmailReplyTo, update.ReplyTo ?? string.Empty);
        Set(Fields.PublicBaseUrl, SystemSettingKeys.EmailPublicBaseUrl, update.PublicBaseUrl);
        Set(Fields.RequireEmailVerification, SystemSettingKeys.EmailRequireVerification, update.RequireEmailVerification.ToString().ToLowerInvariant());

        if (update.ClearPassword)
        {
            Set(Fields.SmtpPassword, SystemSettingKeys.EmailSmtpPassword, string.Empty);
        }
        else if (!string.IsNullOrEmpty(update.SmtpPassword))
        {
            Set(Fields.SmtpPassword, SystemSettingKeys.EmailSmtpPassword, passwordProtector.Protect(update.SmtpPassword));
        }

        if (connectionChanged)
        {
            values[SystemSettingKeys.EmailLastSuccessfulTestAt] = string.Empty;
        }

        return values;
    }
}
