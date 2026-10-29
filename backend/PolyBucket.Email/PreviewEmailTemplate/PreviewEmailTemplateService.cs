using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Email.Templates;

namespace PolyBucket.Email.PreviewEmailTemplate;

public class PreviewEmailTemplateService(
    IEmailTemplateRenderer renderer,
    IEmailBrandingProvider brandingProvider,
    IEmailSettingsResolver settingsResolver) : IPreviewEmailTemplateService
{
    public async Task<RenderedEmail> PreviewAsync(EmailTemplateKey key, CancellationToken cancellationToken = default)
    {
        if (!EmailTemplateCatalog.Templates.TryGetValue(key, out var template))
        {
            throw new NotFoundException($"Email template '{key}' was not found.");
        }

        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        var branding = await brandingProvider.GetBrandingAsync(settings, cancellationToken);
        return renderer.Render(key, template.SampleModel, branding);
    }
}
