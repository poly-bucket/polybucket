using System;
using System.Collections.Generic;
using System.Linq;
using PolyBucket.Api.Common.Email.Templates;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email;

public class EmailTemplateRendererTests
{
    private static readonly EmailBranding Branding = new("Maker Hub", "https://models.example.com");
    private readonly EmailTemplateRenderer _renderer = new();

    public static IEnumerable<object[]> AllTemplates() =>
        Enum.GetValues<EmailTemplateKey>().Select(key => new object[] { key });

    [Theory(DisplayName = "Every template key has a catalog entry that renders with its sample model.")]
    [MemberData(nameof(AllTemplates))]
    public void Render_EveryTemplate_RendersSampleModel(EmailTemplateKey key)
    {
        // Arrange
        var template = EmailTemplateCatalog.Templates[key];

        // Act
        var rendered = _renderer.Render(key, template.SampleModel, Branding);

        // Assert
        rendered.Subject.ShouldNotBeNullOrWhiteSpace();
        rendered.HtmlBody.ShouldContain("Maker Hub");
        rendered.TextBody.ShouldNotBeNullOrWhiteSpace();
        rendered.HtmlBody.ShouldNotContain("{{");
        rendered.TextBody.ShouldNotContain("{{");
    }

    [Fact(DisplayName = "When model values contain HTML, the HTML body encodes them and the text body keeps them as-is.")]
    public void Render_HtmlInModel_IsEncodedInHtmlOnly()
    {
        // Arrange
        var model = new Dictionary<string, string>
        {
            ["username"] = "<script>alert(1)</script>",
            ["actionUrl"] = "https://models.example.com/verify-email?token=abc",
            ["expiresInHours"] = "24"
        };

        // Act
        var rendered = _renderer.Render(EmailTemplateKey.VerifyEmail, model, Branding);

        // Assert
        rendered.HtmlBody.ShouldNotContain("<script>");
        rendered.HtmlBody.ShouldContain("&lt;script&gt;");
        rendered.TextBody.ShouldContain("<script>alert(1)</script>");
    }

    [Fact(DisplayName = "When the action URL is not http or https, the button is omitted.")]
    public void Render_UnsafeActionUrl_OmitsButton()
    {
        // Arrange
        var model = new Dictionary<string, string>
        {
            ["username"] = "maker",
            ["actionUrl"] = "javascript:alert(1)",
            ["expiresInHours"] = "24"
        };

        // Act
        var rendered = _renderer.Render(EmailTemplateKey.VerifyEmail, model, Branding);

        // Assert
        rendered.HtmlBody.ShouldNotContain("javascript:");
        rendered.HtmlBody.ShouldNotContain("<a href");
    }

    [Fact(DisplayName = "When a subject value contains line breaks, they are flattened to prevent header injection.")]
    public void Render_SubjectWithNewlines_IsFlattened()
    {
        // Arrange
        var model = new Dictionary<string, string>
        {
            ["username"] = "maker",
            ["title"] = "Hello\r\nBcc: victim@example.com",
            ["message"] = "Body"
        };

        // Act
        var rendered = _renderer.Render(EmailTemplateKey.Notification, model, Branding);

        // Assert
        rendered.Subject.ShouldNotContain("\n");
        rendered.Subject.ShouldNotContain("\r");
    }

    [Fact(DisplayName = "Templates that carry tokens are flagged so their model is scrubbed after sending.")]
    public void Catalog_TokenTemplates_AreMarkedAsContainingSecrets()
    {
        // Arrange
        var tokenTemplates = new[]
        {
            EmailTemplateKey.VerifyEmail,
            EmailTemplateKey.PasswordReset,
            EmailTemplateKey.AdminCreatedAccount,
            EmailTemplateKey.EmailChangeRequested
        };

        // Act
        var flagged = tokenTemplates.Select(k => EmailTemplateCatalog.Templates[k].ContainsSecrets);

        // Assert
        flagged.ShouldAllBe(isSecret => isSecret);
        EmailTemplateCatalog.Templates[EmailTemplateKey.Welcome].ContainsSecrets.ShouldBeFalse();
    }
}
