using PolyBucket.Email.Templates;
using PolyBucket.Api.Features.Email.PreviewEmailTemplate.Repository;

namespace PolyBucket.Api.Features.Email.PreviewEmailTemplate.Domain;

public class PreviewEmailTemplateService(IPreviewEmailTemplateRepository repository) : IPreviewEmailTemplateService
{
    public Task<RenderedEmail> PreviewAsync(EmailTemplateKey key, CancellationToken cancellationToken = default) =>
        repository.PreviewAsync(key, cancellationToken);
}
