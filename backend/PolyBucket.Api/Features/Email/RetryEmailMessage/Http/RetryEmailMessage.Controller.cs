using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Email.RetryEmailMessage.Domain;

namespace PolyBucket.Api.Features.Email.RetryEmailMessage.Http;

[Authorize]
[ApiController]
[Route("api/admin/email/outbox")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class RetryEmailMessageController(IRetryEmailMessageService service) : ControllerBase
{
    private readonly IRetryEmailMessageService _service = service;

    /// <summary>
    /// Puts a failed or dead-lettered email back in the queue with its attempt count reset.
    /// </summary>
    /// <param name="id">The outbox message ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">The email was requeued.</response>
    /// <response code="404">No email exists with this ID.</response>
    /// <response code="409">The email is not in a retryable state.</response>
    [HttpPost("{id:guid}/retry")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> RetryEmailMessage(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.RetryAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Email not found", Detail = ex.Message, Status = 404 });
        }
        catch (ConflictException ex)
        {
            return Conflict(new ProblemDetails { Title = "Email cannot be retried", Detail = ex.Message, Status = 409 });
        }
    }
}
