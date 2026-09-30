using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Domain;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Http;

[Authorize]
[ApiController]
[Route("api/system-settings/email")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class UpdateEmailSettingsController(IUpdateEmailSettingsService service) : ControllerBase
{
    private readonly IUpdateEmailSettingsService _service = service;

    /// <summary>
    /// Updates the email configuration stored in the database.
    /// </summary>
    /// <remarks>
    /// Leave <c>smtpPassword</c> empty to keep the stored password, or set <c>clearPassword</c> to remove it.
    /// Fields supplied through environment variables cannot be changed here and return 409.
    /// Requiring email verification needs a successful test email within the last 24 hours.
    /// </remarks>
    /// <param name="request">The email settings to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The updated effective email settings.</response>
    /// <response code="400">The settings are invalid.</response>
    /// <response code="409">A field managed by environment variables was changed.</response>
    [HttpPut]
    [ProducesResponseType(typeof(EmailSettingsDto), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<EmailSettingsDto>> UpdateEmailSettings(
        [FromBody] UpdateEmailSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        try
        {
            var result = await _service.UpdateAsync(
                new EmailSettingsUpdate(
                    request.Transport,
                    request.SmtpHost,
                    request.SmtpPort,
                    request.SmtpSecurity,
                    request.SmtpUsername,
                    request.SmtpPassword,
                    request.ClearPassword,
                    request.AllowInvalidCertificates,
                    request.FromAddress,
                    request.FromName,
                    request.ReplyTo,
                    request.PublicBaseUrl,
                    request.RequireEmailVerification),
                cancellationToken);

            return Ok(result);
        }
        catch (DomainValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return BadRequest(new ValidationProblemDetails(ModelState));
        }
        catch (ConflictException ex)
        {
            return Conflict(new ProblemDetails { Title = "Setting managed by environment", Detail = ex.Message, Status = 409 });
        }
    }
}
