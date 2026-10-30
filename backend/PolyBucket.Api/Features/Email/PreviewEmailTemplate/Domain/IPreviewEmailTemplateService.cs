using PolyBucket.Email.Templates;

namespace PolyBucket.Api.Features.Email.PreviewEmailTemplate.Domain;

public interface IPreviewEmailTemplateService
{
    Task<RenderedEmail> PreviewAsync(EmailTemplateKey key, CancellationToken cancellationToken = default);
}
