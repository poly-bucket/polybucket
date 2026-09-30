using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Email.PreviewEmailTemplate.Domain;

namespace PolyBucket.Api.Features.Email.PreviewEmailTemplate.Http;

[Authorize]
[ApiController]
[Route("api/admin/email/templates")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class PreviewEmailTemplateController(IPreviewEmailTemplateService service) : ControllerBase
{
    private readonly IPreviewEmailTemplateService _service = service;

    /// <summary>
    /// Renders an email template with sample data and the current site branding.
    /// </summary>
    /// <param name="key">The template to preview.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The rendered subject, HTML body, and plain-text body.</response>
    /// <response code="404">The template does not exist.</response>
    [HttpGet("{key}/preview")]
    [ProducesResponseType(typeof(RenderedEmail), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<RenderedEmail>> PreviewEmailTemplate(EmailTemplateKey key, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.PreviewAsync(key, cancellationToken));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Template not found", Detail = ex.Message, Status = 404 });
        }
    }
}
