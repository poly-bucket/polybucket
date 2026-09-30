using System.Collections.Generic;

namespace PolyBucket.Api.Common.Email.Templates;

public sealed record EmailTemplateDefinition(
    EmailTemplateKey Key,
    string Subject,
    string HtmlBody,
    string TextBody,
    IReadOnlyList<string> RequiredFields,
    bool ContainsSecrets,
    IReadOnlyDictionary<string, string> SampleModel);

public static class EmailTemplateCatalog
{
    public static readonly IReadOnlyDictionary<EmailTemplateKey, EmailTemplateDefinition> Templates =
        new Dictionary<EmailTemplateKey, EmailTemplateDefinition>
        {
            [EmailTemplateKey.Test] = new(
                EmailTemplateKey.Test,
                "{{siteName}} email configuration test",
                "<h2>Email configuration test</h2><p>If you received this message, email delivery for {{siteName}} is working.</p>",
                "Email configuration test\n\nIf you received this message, email delivery for {{siteName}} is working.",
                [],
                false,
                new Dictionary<string, string>()),

            [EmailTemplateKey.VerifyEmail] = new(
                EmailTemplateKey.VerifyEmail,
                "Verify your {{siteName}} email address",
                "<h2>Verify your email address</h2><p>Hi {{username}},</p><p>Confirm this address to finish setting up your {{siteName}} account.</p>{{button:actionUrl:Verify email}}<p>This link expires in {{expiresInHours}} hours. If you did not create an account, you can ignore this email.</p>",
                "Hi {{username}},\n\nConfirm this address to finish setting up your {{siteName}} account:\n{{actionUrl}}\n\nThis link expires in {{expiresInHours}} hours. If you did not create an account, you can ignore this email.",
                ["username", "actionUrl", "expiresInHours"],
                true,
                new Dictionary<string, string> { ["username"] = "maker42", ["actionUrl"] = "https://example.com/verify-email?token=sample", ["expiresInHours"] = "24" }),

            [EmailTemplateKey.PasswordReset] = new(
                EmailTemplateKey.PasswordReset,
                "Reset your {{siteName}} password",
                "<h2>Reset your password</h2><p>Hi {{username}},</p><p>Someone requested a password reset for your {{siteName}} account.</p>{{button:actionUrl:Choose a new password}}<p>This link expires in {{expiresInMinutes}} minutes and can be used once. If you did not request this, you can ignore this email; your password will not change.</p>",
                "Hi {{username}},\n\nSomeone requested a password reset for your {{siteName}} account. Choose a new password here:\n{{actionUrl}}\n\nThis link expires in {{expiresInMinutes}} minutes and can be used once. If you did not request this, ignore this email.",
                ["username", "actionUrl", "expiresInMinutes"],
                true,
                new Dictionary<string, string> { ["username"] = "maker42", ["actionUrl"] = "https://example.com/reset-password?token=sample", ["expiresInMinutes"] = "60" }),

            [EmailTemplateKey.Welcome] = new(
                EmailTemplateKey.Welcome,
                "Welcome to {{siteName}}",
                "<h2>Welcome to {{siteName}}</h2><p>Hi {{username}},</p><p>Your account is ready. You can start uploading and sharing 3D models.</p>{{button:siteUrl:Start exploring}}",
                "Hi {{username}},\n\nYour {{siteName}} account is ready. You can start uploading and sharing 3D models:\n{{siteUrl}}",
                ["username"],
                false,
                new Dictionary<string, string> { ["username"] = "maker42" }),

            [EmailTemplateKey.AdminCreatedAccount] = new(
                EmailTemplateKey.AdminCreatedAccount,
                "An account was created for you on {{siteName}}",
                "<h2>You've been invited to {{siteName}}</h2><p>Hi {{username}},</p><p>An administrator created a {{siteName}} account for you. Set your password to sign in.</p>{{button:actionUrl:Set your password}}<p>This link expires in {{expiresInHours}} hours. If it expires, use \"Forgot password\" on the sign-in page.</p>",
                "Hi {{username}},\n\nAn administrator created a {{siteName}} account for you. Set your password here:\n{{actionUrl}}\n\nThis link expires in {{expiresInHours}} hours. If it expires, use \"Forgot password\" on the sign-in page.",
                ["username", "actionUrl", "expiresInHours"],
                true,
                new Dictionary<string, string> { ["username"] = "maker42", ["actionUrl"] = "https://example.com/reset-password?token=sample", ["expiresInHours"] = "72" }),

            [EmailTemplateKey.PasswordChanged] = new(
                EmailTemplateKey.PasswordChanged,
                "Your {{siteName}} password was changed",
                "<h2>Your password was changed</h2><p>Hi {{username}},</p><p>The password for your {{siteName}} account was changed on {{changedAt}} (UTC) from IP address {{ipAddress}}. All other sessions were signed out.</p><p>If this wasn't you, reset your password immediately and contact an administrator.</p>",
                "Hi {{username}},\n\nThe password for your {{siteName}} account was changed on {{changedAt}} (UTC) from IP address {{ipAddress}}. All other sessions were signed out.\n\nIf this wasn't you, reset your password immediately and contact an administrator.",
                ["username", "changedAt", "ipAddress"],
                false,
                new Dictionary<string, string> { ["username"] = "maker42", ["changedAt"] = "2026-01-01 12:00", ["ipAddress"] = "203.0.113.7" }),

            [EmailTemplateKey.TwoFactorChanged] = new(
                EmailTemplateKey.TwoFactorChanged,
                "Two-factor authentication was {{state}} on your {{siteName}} account",
                "<h2>Two-factor authentication {{state}}</h2><p>Hi {{username}},</p><p>Two-factor authentication was {{state}} for your {{siteName}} account on {{changedAt}} (UTC) from IP address {{ipAddress}}.</p><p>If this wasn't you, change your password immediately and contact an administrator.</p>",
                "Hi {{username}},\n\nTwo-factor authentication was {{state}} for your {{siteName}} account on {{changedAt}} (UTC) from IP address {{ipAddress}}.\n\nIf this wasn't you, change your password immediately and contact an administrator.",
                ["username", "state", "changedAt", "ipAddress"],
                false,
                new Dictionary<string, string> { ["username"] = "maker42", ["state"] = "enabled", ["changedAt"] = "2026-01-01 12:00", ["ipAddress"] = "203.0.113.7" }),

            [EmailTemplateKey.EmailChangeRequested] = new(
                EmailTemplateKey.EmailChangeRequested,
                "Confirm your new {{siteName}} email address",
                "<h2>Confirm your new email address</h2><p>Hi {{username}},</p><p>Confirm this address to make it the sign-in email for your {{siteName}} account.</p>{{button:actionUrl:Confirm new email}}<p>This link expires in {{expiresInHours}} hours. Your current address stays active until you confirm.</p>",
                "Hi {{username}},\n\nConfirm this address to make it the sign-in email for your {{siteName}} account:\n{{actionUrl}}\n\nThis link expires in {{expiresInHours}} hours. Your current address stays active until you confirm.",
                ["username", "actionUrl", "expiresInHours"],
                true,
                new Dictionary<string, string> { ["username"] = "maker42", ["actionUrl"] = "https://example.com/verify-email?token=sample", ["expiresInHours"] = "24" }),

            [EmailTemplateKey.EmailChangedNotice] = new(
                EmailTemplateKey.EmailChangedNotice,
                "Your {{siteName}} email address was changed",
                "<h2>Your email address was changed</h2><p>Hi {{username}},</p><p>The sign-in email for your {{siteName}} account was changed to {{newEmail}}. This address will no longer receive account emails.</p><p>If this wasn't you, contact an administrator immediately.</p>",
                "Hi {{username}},\n\nThe sign-in email for your {{siteName}} account was changed to {{newEmail}}. This address will no longer receive account emails.\n\nIf this wasn't you, contact an administrator immediately.",
                ["username", "newEmail"],
                false,
                new Dictionary<string, string> { ["username"] = "maker42", ["newEmail"] = "m***@example.com" }),

            [EmailTemplateKey.Notification] = new(
                EmailTemplateKey.Notification,
                "{{title}}",
                "<h2>{{title}}</h2><p>Hi {{username}},</p><p>{{message}}</p>{{button:actionUrl:View details}}<p style=\"font-size:12px;color:#6b7280\">You can change which emails you receive in your notification settings.</p>",
                "Hi {{username}},\n\n{{message}}\n\n{{actionUrl}}\n\nYou can change which emails you receive in your notification settings.",
                ["username", "title", "message"],
                false,
                new Dictionary<string, string> { ["username"] = "maker42", ["title"] = "Your model was approved", ["message"] = "Benchy was approved and is now public.", ["actionUrl"] = "https://example.com/models/1" })
        };
}
