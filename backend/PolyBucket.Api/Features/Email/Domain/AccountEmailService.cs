using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Email.Domain;

public interface IAccountEmailService
{
    Task<EmailEnqueueOutcome> SendVerificationAsync(User user, string rawToken, TimeSpan lifetime, bool saveChanges = true, CancellationToken cancellationToken = default);
    Task<EmailEnqueueOutcome> SendPasswordResetAsync(User user, string rawToken, TimeSpan lifetime, bool saveChanges = true, CancellationToken cancellationToken = default);
    Task<EmailEnqueueOutcome> SendWelcomeAsync(User user, bool saveChanges = true, CancellationToken cancellationToken = default);
    Task<EmailEnqueueOutcome> SendAccountInviteAsync(User user, string rawToken, TimeSpan lifetime, bool saveChanges = true, CancellationToken cancellationToken = default);
    Task<EmailEnqueueOutcome> SendPasswordChangedAsync(User user, ClientRequestInfo client, bool saveChanges = true, CancellationToken cancellationToken = default);
    Task<EmailEnqueueOutcome> SendTwoFactorChangedAsync(User user, bool enabled, ClientRequestInfo client, bool saveChanges = true, CancellationToken cancellationToken = default);
    Task<EmailEnqueueOutcome> SendEmailChangeRequestedAsync(User user, string newEmail, string rawToken, TimeSpan lifetime, bool saveChanges = true, CancellationToken cancellationToken = default);
    Task<EmailEnqueueOutcome> SendEmailChangedNoticeAsync(User user, string previousEmail, string newEmail, bool saveChanges = true, CancellationToken cancellationToken = default);
}

public class AccountEmailService(
    IEmailQueue queue,
    IEmailSettingsResolver settingsResolver,
    TimeProvider timeProvider,
    ILogger<AccountEmailService> logger) : IAccountEmailService
{
    public const string VerifyEmailPath = "/verify-email";
    public const string ResetPasswordPath = "/reset-password";

    public Task<EmailEnqueueOutcome> SendVerificationAsync(User user, string rawToken, TimeSpan lifetime, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return SendWithLinkAsync(
            EmailTemplateKey.VerifyEmail,
            user.Email,
            $"{VerifyEmailPath}?token={Uri.EscapeDataString(rawToken)}",
            new Dictionary<string, string>
            {
                ["username"] = user.Username,
                ["expiresInHours"] = FormatHours(lifetime)
            },
            saveChanges,
            cancellationToken);
    }

    public Task<EmailEnqueueOutcome> SendPasswordResetAsync(User user, string rawToken, TimeSpan lifetime, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return SendWithLinkAsync(
            EmailTemplateKey.PasswordReset,
            user.Email,
            $"{ResetPasswordPath}?token={Uri.EscapeDataString(rawToken)}",
            new Dictionary<string, string>
            {
                ["username"] = user.Username,
                ["expiresInMinutes"] = ((int)Math.Round(lifetime.TotalMinutes)).ToString(CultureInfo.InvariantCulture)
            },
            saveChanges,
            cancellationToken);
    }

    public Task<EmailEnqueueOutcome> SendWelcomeAsync(User user, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return queue.EnqueueAsync(
            new EmailRequest(
                EmailTemplateKey.Welcome,
                user.Email,
                new Dictionary<string, string> { ["username"] = user.Username },
                $"welcome:{user.Id}"),
            saveChanges,
            cancellationToken);
    }

    public Task<EmailEnqueueOutcome> SendAccountInviteAsync(User user, string rawToken, TimeSpan lifetime, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return SendWithLinkAsync(
            EmailTemplateKey.AdminCreatedAccount,
            user.Email,
            $"{ResetPasswordPath}?token={Uri.EscapeDataString(rawToken)}&invite=1",
            new Dictionary<string, string>
            {
                ["username"] = user.Username,
                ["expiresInHours"] = FormatHours(lifetime)
            },
            saveChanges,
            cancellationToken);
    }

    public Task<EmailEnqueueOutcome> SendPasswordChangedAsync(User user, ClientRequestInfo client, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return queue.EnqueueAsync(
            new EmailRequest(
                EmailTemplateKey.PasswordChanged,
                user.Email,
                new Dictionary<string, string>
                {
                    ["username"] = user.Username,
                    ["changedAt"] = FormatNow(),
                    ["ipAddress"] = client.IpAddress
                }),
            saveChanges,
            cancellationToken);
    }

    public Task<EmailEnqueueOutcome> SendTwoFactorChangedAsync(User user, bool enabled, ClientRequestInfo client, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return queue.EnqueueAsync(
            new EmailRequest(
                EmailTemplateKey.TwoFactorChanged,
                user.Email,
                new Dictionary<string, string>
                {
                    ["username"] = user.Username,
                    ["state"] = enabled ? "enabled" : "disabled",
                    ["changedAt"] = FormatNow(),
                    ["ipAddress"] = client.IpAddress
                }),
            saveChanges,
            cancellationToken);
    }

    public Task<EmailEnqueueOutcome> SendEmailChangeRequestedAsync(User user, string newEmail, string rawToken, TimeSpan lifetime, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return SendWithLinkAsync(
            EmailTemplateKey.EmailChangeRequested,
            newEmail,
            $"{VerifyEmailPath}?token={Uri.EscapeDataString(rawToken)}&type=email-change",
            new Dictionary<string, string>
            {
                ["username"] = user.Username,
                ["expiresInHours"] = FormatHours(lifetime)
            },
            saveChanges,
            cancellationToken);
    }

    public Task<EmailEnqueueOutcome> SendEmailChangedNoticeAsync(User user, string previousEmail, string newEmail, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        return queue.EnqueueAsync(
            new EmailRequest(
                EmailTemplateKey.EmailChangedNotice,
                previousEmail,
                new Dictionary<string, string>
                {
                    ["username"] = user.Username,
                    ["newEmail"] = MaskEmail(newEmail)
                }),
            saveChanges,
            cancellationToken);
    }

    public static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 0)
        {
            return "***";
        }

        return email[0] + "***" + email[at..];
    }

    private async Task<EmailEnqueueOutcome> SendWithLinkAsync(
        EmailTemplateKey template,
        string recipient,
        string relativeUrl,
        Dictionary<string, string> model,
        bool saveChanges,
        CancellationToken cancellationToken)
    {
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        if (!settings.IsEnabled)
        {
            logger.LogWarning("Email delivery is disabled; {Template} email was not queued", template);
            return EmailEnqueueOutcome.EmailDisabled;
        }

        if (!settings.HasPublicBaseUrl)
        {
            logger.LogError("Cannot queue {Template} email because Email:PublicBaseUrl is not configured", template);
            return EmailEnqueueOutcome.MissingPublicBaseUrl;
        }

        model["actionUrl"] = settings.BuildUrl(relativeUrl);
        return await queue.EnqueueAsync(new EmailRequest(template, recipient, model), saveChanges, cancellationToken);
    }

    private static string FormatHours(TimeSpan lifetime) =>
        ((int)Math.Round(lifetime.TotalHours)).ToString(CultureInfo.InvariantCulture);

    private string FormatNow() =>
        timeProvider.GetUtcNow().UtcDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
}
