using PolyBucket.Email.Templates;
using PolyBucket.Email.PreviewEmailTemplate;
using Inner = PolyBucket.Email.PreviewEmailTemplate.IPreviewEmailTemplateService;

namespace PolyBucket.Api.Features.Email.PreviewEmailTemplate.Repository;

public class PreviewEmailTemplateRepository(Inner inner) : IPreviewEmailTemplateRepository
{
    public Task<RenderedEmail> PreviewAsync(EmailTemplateKey key, CancellationToken cancellationToken = default) =>
        inner.PreviewAsync(key, cancellationToken);
}
