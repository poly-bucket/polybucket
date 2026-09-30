using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Domain;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using Xunit;
using Fields = PolyBucket.Api.Common.Email.EffectiveEmailSettings.Fields;

namespace PolyBucket.Tests.Features.Email;

public class UpdateEmailSettingsServiceTests
{
    private readonly Mock<IUpdateEmailSettingsRepository> _repository = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly SmtpPasswordProtector _protector = new(new EphemeralDataProtectionProvider());
    private IReadOnlyDictionary<string, string>? _saved;

    public UpdateEmailSettingsServiceTests()
    {
        _repository.Setup(r => r.UpsertAsync(It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyDictionary<string, string>, CancellationToken>((values, _) => _saved = values)
            .Returns(Task.CompletedTask);
    }

    private UpdateEmailSettingsService CreateService(EffectiveEmailSettings current)
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(current);
        return new UpdateEmailSettingsService(_repository.Object, _resolver.Object, _protector, TimeProvider.System, NullLogger<UpdateEmailSettingsService>.Instance);
    }

    private static EmailSettingsUpdate SmtpUpdate(string? password = null, bool clearPassword = false, bool requireVerification = false, string host = "smtp.example.com") =>
        new(EmailTransportKind.Smtp, host, 587, EmailSecurityMode.StartTls, "user", password, clearPassword, false,
            "noreply@example.com", "PolyBucket", null, "https://models.example.com", requireVerification);

    private static EffectiveEmailSettings CurrentSmtp(DateTime? lastTest = null, Dictionary<string, EmailSettingSource>? sources = null) => new()
    {
        Transport = EmailTransportKind.Smtp,
        SmtpHost = "smtp.example.com",
        SmtpPort = 587,
        SmtpSecurity = EmailSecurityMode.StartTls,
        SmtpUsername = "user",
        SmtpPassword = "existing",
        FromAddress = "noreply@example.com",
        FromName = "PolyBucket",
        PublicBaseUrl = "https://models.example.com",
        LastSuccessfulTestAt = lastTest,
        Sources = sources ?? new Dictionary<string, EmailSettingSource>()
    };

    [Fact(DisplayName = "When a new password is provided, it is stored encrypted and the connection test timestamp is cleared.")]
    public async Task Update_NewPassword_IsEncrypted()
    {
        // Arrange
        var service = CreateService(CurrentSmtp(DateTime.UtcNow));

        // Act
        await service.UpdateAsync(SmtpUpdate(password: "new-secret"));

        // Assert
        _saved![SystemSettingKeys.EmailSmtpPassword].ShouldStartWith(SmtpPasswordProtector.Prefix);
        _saved[SystemSettingKeys.EmailSmtpPassword].ShouldNotContain("new-secret");
        _saved[SystemSettingKeys.EmailLastSuccessfulTestAt].ShouldBeEmpty();
        _resolver.Verify(r => r.Invalidate(), Times.Once);
    }

    [Fact(DisplayName = "When the password field is left blank, the stored password is not touched.")]
    public async Task Update_BlankPassword_KeepsExisting()
    {
        // Arrange
        var service = CreateService(CurrentSmtp());

        // Act
        await service.UpdateAsync(SmtpUpdate(password: null));

        // Assert
        _saved!.ContainsKey(SystemSettingKeys.EmailSmtpPassword).ShouldBeFalse();
    }

    [Fact(DisplayName = "When clearPassword is set, the stored password is erased.")]
    public async Task Update_ClearPassword_ErasesPassword()
    {
        // Arrange
        var service = CreateService(CurrentSmtp());

        // Act
        await service.UpdateAsync(SmtpUpdate(clearPassword: true));

        // Assert
        _saved![SystemSettingKeys.EmailSmtpPassword].ShouldBeEmpty();
    }

    [Fact(DisplayName = "When a field managed by environment variables is changed, the update is rejected with a conflict.")]
    public async Task Update_ChangingEnvironmentManagedField_ThrowsConflict()
    {
        // Arrange
        var service = CreateService(CurrentSmtp(sources: new Dictionary<string, EmailSettingSource> { [Fields.SmtpHost] = EmailSettingSource.Environment }));

        // Act
        var exception = await Should.ThrowAsync<ConflictException>(() => service.UpdateAsync(SmtpUpdate(host: "smtp.other.example")));

        // Assert
        exception.Message.ShouldContain(Fields.SmtpHost);
        _repository.Verify(r => r.UpsertAsync(It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When environment-managed fields are sent back unchanged, they are accepted but not written to the database.")]
    public async Task Update_UnchangedEnvironmentField_IsSkipped()
    {
        // Arrange
        var service = CreateService(CurrentSmtp(sources: new Dictionary<string, EmailSettingSource> { [Fields.SmtpHost] = EmailSettingSource.Environment }));

        // Act
        await service.UpdateAsync(SmtpUpdate());

        // Assert
        _saved!.ContainsKey(SystemSettingKeys.EmailSmtpServer).ShouldBeFalse();
        _saved.ContainsKey(SystemSettingKeys.EmailFromAddress).ShouldBeTrue();
    }

    [Fact(DisplayName = "When email verification is turned on without a recent successful test, the update is rejected.")]
    public async Task Update_RequireVerificationWithoutRecentTest_ThrowsValidation()
    {
        // Arrange
        var service = CreateService(CurrentSmtp(lastTest: DateTime.UtcNow.AddDays(-3)));

        // Act
        var exception = await Should.ThrowAsync<DomainValidationException>(() => service.UpdateAsync(SmtpUpdate(requireVerification: true)));

        // Assert
        exception.Errors.ShouldContain(e => e.Contains("successful test email"));
    }

    [Fact(DisplayName = "When email verification is turned on after a test in the last 24 hours, the update succeeds.")]
    public async Task Update_RequireVerificationWithRecentTest_Succeeds()
    {
        // Arrange
        var service = CreateService(CurrentSmtp(lastTest: DateTime.UtcNow.AddHours(-1)));

        // Act
        await service.UpdateAsync(SmtpUpdate(requireVerification: true));

        // Assert
        _saved![SystemSettingKeys.EmailRequireVerification].ShouldBe("true");
    }

    [Fact(DisplayName = "When SMTP is chosen without a host or valid From address, the update is rejected with validation errors.")]
    public async Task Update_InvalidSmtp_ThrowsValidation()
    {
        // Arrange
        var service = CreateService(new EffectiveEmailSettings());
        var update = SmtpUpdate(host: string.Empty) with { FromAddress = "nope" };

        // Act
        var exception = await Should.ThrowAsync<DomainValidationException>(() => service.UpdateAsync(update));

        // Assert
        exception.Errors.ShouldContain(e => e.Contains("SMTP host"));
        exception.Errors.ShouldContain(e => e.Contains("From address"));
    }
}
