using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.GetEmailSettings.Domain;

namespace PolyBucket.Api.Features.Email.GetEmailSettings.Http;

[Authorize]
[ApiController]
[Route("api/system-settings/email")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class GetEmailSettingsController(IGetEmailSettingsService service) : ControllerBase
{
    private readonly IGetEmailSettingsService _service = service;

    /// <summary>
    /// Gets the effective email configuration, including where each value comes from and which fields are managed by environment variables.
    /// </summary>
    /// <remarks>The SMTP password is never returned; <c>hasPassword</c> indicates whether one is stored.</remarks>
    /// <response code="200">The effective email settings.</response>
    [HttpGet]
    [ProducesResponseType(typeof(EmailSettingsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<EmailSettingsDto>> GetEmailSettings(CancellationToken cancellationToken = default)
    {
        return Ok(await _service.GetAsync(cancellationToken));
    }
}
