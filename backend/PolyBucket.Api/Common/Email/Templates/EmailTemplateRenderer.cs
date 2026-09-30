using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace PolyBucket.Api.Common.Email.Templates;

public sealed record EmailBranding(string SiteName, string SiteUrl);

public sealed record RenderedEmail(string Subject, string HtmlBody, string TextBody);

public interface IEmailTemplateRenderer
{
    RenderedEmail Render(EmailTemplateKey key, IReadOnlyDictionary<string, string> model, EmailBranding branding);
}

public partial class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Default;

    [GeneratedRegex(@"\{\{button:(\w+):([^}]+)\}\}")]
    private static partial Regex ButtonPattern();

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderPattern();

    public RenderedEmail Render(EmailTemplateKey key, IReadOnlyDictionary<string, string> model, EmailBranding branding)
    {
        if (!EmailTemplateCatalog.Templates.TryGetValue(key, out var template))
        {
            throw new InvalidOperationException($"No email template is registered for '{key}'.");
        }

        var values = new Dictionary<string, string>(model, StringComparer.OrdinalIgnoreCase)
        {
            ["siteName"] = branding.SiteName,
            ["siteUrl"] = branding.SiteUrl
        };

        var subject = ReplacePlaceholders(template.Subject, values, encode: false).ReplaceLineEndings(" ");
        var htmlContent = RenderButtons(template.HtmlBody, values);
        htmlContent = ReplacePlaceholders(htmlContent, values, encode: true);
        var text = ReplacePlaceholders(template.TextBody, values, encode: false)
            + $"\n\n-- \n{branding.SiteName}" + (string.IsNullOrWhiteSpace(branding.SiteUrl) ? string.Empty : $"\n{branding.SiteUrl}");

        return new RenderedEmail(subject, WrapInLayout(htmlContent, branding), text);
    }

    private static string RenderButtons(string html, IReadOnlyDictionary<string, string> values)
    {
        return ButtonPattern().Replace(html, match =>
        {
            var url = values.TryGetValue(match.Groups[1].Value, out var value) ? value : null;
            if (!IsSafeUrl(url))
            {
                return string.Empty;
            }

            var label = Encoder.Encode(match.Groups[2].Value);
            var href = Encoder.Encode(url!);
            return $"<p style=\"margin:24px 0\"><a href=\"{href}\" style=\"display:inline-block;padding:12px 20px;background:#2563eb;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600\">{label}</a></p>"
                + $"<p style=\"font-size:12px;color:#6b7280\">If the button doesn't work, copy this link into your browser:<br><span style=\"word-break:break-all\">{href}</span></p>";
        });
    }

    private static string ReplacePlaceholders(string template, IReadOnlyDictionary<string, string> values, bool encode)
    {
        return PlaceholderPattern().Replace(template, match =>
        {
            var value = values.TryGetValue(match.Groups[1].Value, out var found) ? found ?? string.Empty : string.Empty;
            return encode ? Encoder.Encode(value) : value;
        });
    }

    private static bool IsSafeUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }

    private static string WrapInLayout(string content, EmailBranding branding)
    {
        var siteName = Encoder.Encode(branding.SiteName);
        return "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"></head>"
            + "<body style=\"margin:0;padding:0;background:#f3f4f6;font-family:-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#111827\">"
            + "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\"><tr><td align=\"center\" style=\"padding:24px\">"
            + "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:560px;background:#ffffff;border-radius:8px\">"
            + $"<tr><td style=\"padding:20px 24px;border-bottom:1px solid #e5e7eb;font-size:18px;font-weight:700\">{siteName}</td></tr>"
            + $"<tr><td style=\"padding:24px;font-size:15px;line-height:1.6\">{content}</td></tr>"
            + $"<tr><td style=\"padding:16px 24px;border-top:1px solid #e5e7eb;font-size:12px;color:#6b7280\">Sent by {siteName}</td></tr>"
            + "</table></td></tr></table></body></html>";
    }
}
