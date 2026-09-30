using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Common.Email;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Email;

[Collection("MailpitCollection")]
public class SmtpEmailTransportMailpitTests(MailpitFixture mailpit)
{
    private readonly SmtpEmailTransport _transport = new(NullLogger<SmtpEmailTransport>.Instance);

    private EffectiveEmailSettings Settings(string password = MailpitFixture.Password) => new()
    {
        Transport = EmailTransportKind.Smtp,
        SmtpHost = mailpit.Host,
        SmtpPort = mailpit.Port,
        SmtpSecurity = EmailSecurityMode.None,
        SmtpUsername = MailpitFixture.Username,
        SmtpPassword = password,
        FromAddress = "noreply@polybucket.test",
        FromName = "PolyBucket Test",
        ReplyTo = "support@polybucket.test",
        TimeoutSeconds = 10
    };

    private static EmailEnvelope Envelope(string to) => new()
    {
        MessageId = Guid.NewGuid(),
        To = to,
        Subject = "Mailpit delivery test",
        HtmlBody = "<p>Hello <strong>maker</strong></p>",
        TextBody = "Hello maker",
        Headers = new Dictionary<string, string> { ["X-PolyBucket-Template"] = "Test" }
    };

    [Fact(DisplayName = "When SMTP credentials are correct, the message reaches the server with both HTML and text parts, Reply-To, and custom headers.")]
    public async Task SendAsync_ValidCredentials_DeliversMultipartMessage()
    {
        // Arrange
        await mailpit.ClearAsync();
        var envelope = Envelope("recipient@polybucket.test");

        // Act
        await _transport.SendAsync(envelope, Settings());

        // Assert
        var list = await mailpit.GetMessagesAsync();
        var summary = list.GetProperty("messages").EnumerateArray().ShouldHaveSingleItem();
        summary.GetProperty("Subject").GetString().ShouldBe("Mailpit delivery test");
        var message = await mailpit.GetMessageAsync(summary.GetProperty("ID").GetString()!);
        message.GetProperty("HTML").GetString()!.ShouldContain("<strong>maker</strong>");
        message.GetProperty("Text").GetString()!.ShouldContain("Hello maker");
        message.GetProperty("ReplyTo").EnumerateArray().First().GetProperty("Address").GetString().ShouldBe("support@polybucket.test");
        message.GetProperty("MessageID").GetString()!.ShouldEndWith("@polybucket.test");
    }

    [Fact(DisplayName = "When the SMTP password is wrong, sending fails with a permanent delivery error so it is not retried.")]
    public async Task SendAsync_WrongPassword_ThrowsPermanentFailure()
    {
        // Arrange
        await mailpit.ClearAsync();

        // Act
        var exception = await Should.ThrowAsync<EmailDeliveryException>(() => _transport.SendAsync(Envelope("recipient@polybucket.test"), Settings("wrong")));

        // Assert
        exception.IsTransient.ShouldBeFalse();
        exception.Message.ShouldContain("authentication failed");
    }

    [Fact(DisplayName = "When the diagnostic runs with valid settings, every stage succeeds and the test message is delivered.")]
    public async Task DiagnoseAsync_ValidSettings_AllStagesPass()
    {
        // Arrange
        await mailpit.ClearAsync();

        // Act
        var result = await _transport.DiagnoseAsync(Envelope("admin@polybucket.test"), Settings());

        // Assert
        result.Success.ShouldBeTrue();
        result.Stages.Select(s => s.Stage).ShouldBe(
        [
            EmailDiagnosticStage.Configuration,
            EmailDiagnosticStage.Dns,
            EmailDiagnosticStage.Connect,
            EmailDiagnosticStage.Tls,
            EmailDiagnosticStage.Auth,
            EmailDiagnosticStage.Send
        ]);
        (await mailpit.GetMessagesAsync()).GetProperty("messages_count").GetInt32().ShouldBe(1);
    }

    [Fact(DisplayName = "When the diagnostic runs with a wrong password, it stops at the authentication stage.")]
    public async Task DiagnoseAsync_WrongPassword_FailsAtAuth()
    {
        // Arrange
        await mailpit.ClearAsync();

        // Act
        var result = await _transport.DiagnoseAsync(Envelope("admin@polybucket.test"), Settings("wrong"));

        // Assert
        result.Success.ShouldBeFalse();
        result.Stages.Last().Stage.ShouldBe(EmailDiagnosticStage.Auth);
        result.Stages.Last().Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "When nothing is listening on the port, the diagnostic stops at the connect stage.")]
    public async Task DiagnoseAsync_ClosedPort_FailsAtConnect()
    {
        // Arrange
        var settings = Settings() with { SmtpHost = "127.0.0.1", SmtpPort = 1, TimeoutSeconds = 5 };

        // Act
        var result = await _transport.DiagnoseAsync(Envelope("admin@polybucket.test"), settings);

        // Assert
        result.Success.ShouldBeFalse();
        result.Stages.Last().Stage.ShouldBe(EmailDiagnosticStage.Connect);
    }
}
