using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Common.Email.Templates;

public interface IEmailBrandingProvider
{
    Task<EmailBranding> GetBrandingAsync(EffectiveEmailSettings settings, CancellationToken cancellationToken = default);
}

public class EmailBrandingProvider(PolyBucketDbContext context) : IEmailBrandingProvider
{
    public async Task<EmailBranding> GetBrandingAsync(EffectiveEmailSettings settings, CancellationToken cancellationToken = default)
    {
        var siteName = await context.SystemSetups
            .AsNoTracking()
            .Select(s => s.SiteName)
            .FirstOrDefaultAsync(cancellationToken);

        return new EmailBranding(
            string.IsNullOrWhiteSpace(siteName) ? "PolyBucket" : siteName,
            settings.HasPublicBaseUrl ? settings.PublicBaseUrl.TrimEnd('/') : string.Empty);
    }
}
