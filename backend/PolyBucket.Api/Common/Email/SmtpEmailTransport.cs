using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace PolyBucket.Api.Common.Email;

public class SmtpEmailTransport(ILogger<SmtpEmailTransport> logger) : IEmailTransport
{
    public EmailTransportKind Kind => EmailTransportKind.Smtp;

    public async Task SendAsync(EmailEnvelope envelope, EffectiveEmailSettings settings, CancellationToken cancellationToken = default)
    {
        var message = BuildMessage(envelope, settings);

        using var client = CreateClient(settings);
        try
        {
            await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, MapSecurity(settings.SmtpSecurity), cancellationToken);
            if (!string.IsNullOrEmpty(settings.SmtpUsername))
            {
                await client.AuthenticateAsync(settings.SmtpUsername, settings.SmtpPassword, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (AuthenticationException ex)
        {
            throw new EmailDeliveryException("SMTP authentication failed. Check the username and password.", false, ex);
        }
        catch (SmtpCommandException ex) when ((int)ex.StatusCode >= 500)
        {
            throw new EmailDeliveryException($"SMTP server rejected the message ({(int)ex.StatusCode}): {ex.Message}", false, ex);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested
            && ex is SmtpCommandException or SmtpProtocolException or SocketException or IOException or SslHandshakeException or TimeoutException or OperationCanceledException)
        {
            throw new EmailDeliveryException($"SMTP delivery failed: {ex.Message}", true, ex);
        }

        logger.LogInformation("Email {MessageId} delivered via SMTP host {Host}", envelope.MessageId, settings.SmtpHost);
    }

    public async Task<EmailDiagnosticResult> DiagnoseAsync(EmailEnvelope envelope, EffectiveEmailSettings settings, CancellationToken cancellationToken = default)
    {
        var stages = new List<EmailDiagnosticStageResult>();
        var stopwatch = Stopwatch.StartNew();

        var configErrors = settings.GetValidationErrors();
        if (configErrors.Count > 0)
        {
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Configuration, false, string.Join(" ", configErrors), 0));
            return new EmailDiagnosticResult(false, stages);
        }

        stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Configuration, true, "Configuration is valid.", 0));

        stopwatch.Restart();
        try
        {
            if (!IPAddress.TryParse(settings.SmtpHost, out _))
            {
                var addresses = await Dns.GetHostAddressesAsync(settings.SmtpHost, cancellationToken);
                stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Dns, true, $"Resolved {settings.SmtpHost} to {addresses.Length} address(es).", stopwatch.ElapsedMilliseconds));
            }
            else
            {
                stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Dns, true, "Host is an IP address; no lookup needed.", stopwatch.ElapsedMilliseconds));
            }
        }
        catch (SocketException ex)
        {
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Dns, false, $"Could not resolve {settings.SmtpHost}: {ex.Message}", stopwatch.ElapsedMilliseconds));
            return new EmailDiagnosticResult(false, stages);
        }

        using var client = CreateClient(settings);

        stopwatch.Restart();
        try
        {
            await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, MapSecurity(settings.SmtpSecurity), cancellationToken);
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Connect, true, $"Connected to {settings.SmtpHost}:{settings.SmtpPort}.", stopwatch.ElapsedMilliseconds));
            stages.Add(new EmailDiagnosticStageResult(
                EmailDiagnosticStage.Tls,
                true,
                client.IsSecure ? $"Connection is encrypted ({client.SslProtocol})." : "Connection is not encrypted (security mode None or server did not offer STARTTLS).",
                0));
        }
        catch (SslHandshakeException ex)
        {
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Connect, true, $"Connected to {settings.SmtpHost}:{settings.SmtpPort}.", stopwatch.ElapsedMilliseconds));
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Tls, false, $"TLS handshake failed: {ex.Message} Check the security mode for this port (465 uses SSL on connect, 587 uses STARTTLS).", 0));
            return new EmailDiagnosticResult(false, stages);
        }
        catch (Exception ex) when (ex is SocketException or IOException or TimeoutException or SmtpProtocolException or SmtpCommandException or NotSupportedException)
        {
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Connect, false, $"Could not connect to {settings.SmtpHost}:{settings.SmtpPort}: {ex.Message}", stopwatch.ElapsedMilliseconds));
            return new EmailDiagnosticResult(false, stages);
        }

        stopwatch.Restart();
        if (string.IsNullOrEmpty(settings.SmtpUsername))
        {
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Auth, true, "No username configured; skipping authentication.", 0));
        }
        else
        {
            try
            {
                await client.AuthenticateAsync(settings.SmtpUsername, settings.SmtpPassword, cancellationToken);
                stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Auth, true, $"Authenticated as {settings.SmtpUsername}.", stopwatch.ElapsedMilliseconds));
            }
            catch (Exception ex) when (ex is AuthenticationException or SmtpCommandException or SmtpProtocolException or NotSupportedException)
            {
                stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Auth, false, $"Authentication failed: {ex.Message}", stopwatch.ElapsedMilliseconds));
                return new EmailDiagnosticResult(false, stages);
            }
        }

        stopwatch.Restart();
        try
        {
            await client.SendAsync(BuildMessage(envelope, settings), cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Send, true, $"Test message accepted for {envelope.To}.", stopwatch.ElapsedMilliseconds));
        }
        catch (Exception ex) when (ex is SmtpCommandException or SmtpProtocolException or IOException)
        {
            stages.Add(new EmailDiagnosticStageResult(EmailDiagnosticStage.Send, false, $"Server rejected the test message: {ex.Message}", stopwatch.ElapsedMilliseconds));
            return new EmailDiagnosticResult(false, stages);
        }

        return new EmailDiagnosticResult(true, stages);
    }

    internal static MimeMessage BuildMessage(EmailEnvelope envelope, EffectiveEmailSettings settings)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(envelope.To));
        if (!string.IsNullOrWhiteSpace(settings.ReplyTo))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(settings.ReplyTo));
        }

        message.Subject = envelope.Subject;
        message.MessageId = $"{envelope.MessageId:N}@{GetDomain(settings.FromAddress)}";

        foreach (var header in envelope.Headers)
        {
            message.Headers.Replace(header.Key, header.Value);
        }

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = envelope.HtmlBody,
            TextBody = envelope.TextBody
        };
        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }

    private static SmtpClient CreateClient(EffectiveEmailSettings settings)
    {
        var client = new SmtpClient
        {
            Timeout = (int)TimeSpan.FromSeconds(settings.TimeoutSeconds).TotalMilliseconds
        };

        if (settings.AllowInvalidCertificates)
        {
            client.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }

        return client;
    }

    internal static SecureSocketOptions MapSecurity(EmailSecurityMode mode) => mode switch
    {
        EmailSecurityMode.None => SecureSocketOptions.None,
        EmailSecurityMode.StartTls => SecureSocketOptions.StartTls,
        EmailSecurityMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
        _ => SecureSocketOptions.Auto
    };

    private static string GetDomain(string address)
    {
        var at = address.LastIndexOf('@');
        return at >= 0 && at < address.Length - 1 ? address[(at + 1)..] : "polybucket.local";
    }
}
