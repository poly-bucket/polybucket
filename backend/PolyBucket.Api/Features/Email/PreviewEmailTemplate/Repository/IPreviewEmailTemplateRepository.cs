using PolyBucket.Email.Templates;

namespace PolyBucket.Api.Features.Email.PreviewEmailTemplate.Repository;

public interface IPreviewEmailTemplateRepository
{
    Task<RenderedEmail> PreviewAsync(EmailTemplateKey key, CancellationToken cancellationToken = default);
}
