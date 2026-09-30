using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.GetEmailOutbox.Domain;

namespace PolyBucket.Api.Features.Email.GetEmailOutbox.Http;

[Authorize]
[ApiController]
[Route("api/admin/email/outbox")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class GetEmailOutboxController(IGetEmailOutboxService service) : ControllerBase
{
    private readonly IGetEmailOutboxService _service = service;

    /// <summary>
    /// Lists queued, sent, failed, and dead-lettered emails, newest first, with a count per status.
    /// </summary>
    /// <remarks>Message bodies are not returned; links containing tokens are removed from sent messages.</remarks>
    /// <param name="status">Optional status filter.</param>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Items per page, up to 100.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">A page of outbox messages.</response>
    [HttpGet]
    [ProducesResponseType(typeof(EmailOutboxPageDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<EmailOutboxPageDto>> GetEmailOutbox(
        [FromQuery] EmailMessageStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _service.GetAsync(status, page, pageSize, cancellationToken));
    }
}
