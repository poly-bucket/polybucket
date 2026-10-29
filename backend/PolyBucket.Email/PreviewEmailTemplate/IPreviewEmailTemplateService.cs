using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Email.Templates;

namespace PolyBucket.Email.PreviewEmailTemplate;

public interface IPreviewEmailTemplateService
{
    Task<RenderedEmail> PreviewAsync(EmailTemplateKey key, CancellationToken cancellationToken = default);
}
