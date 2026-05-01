using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Authentication.Account.Domain;

namespace PolyBucket.Api.Features.Authentication.Account.Http;

[ApiController]
[Route("api/auth/account")]
[Authorize]
[RequirePermission(PermissionConstants.USER_DELETE_ACCOUNT)]
public class DeleteAccountController(DeleteOwnAccountService deleteOwnAccountService) : ControllerBase
{
    private readonly DeleteOwnAccountService _deleteOwnAccountService = deleteOwnAccountService;

    /// <summary>
    /// Permanently close the current user's account (credentials invalidated and profile anonymized).
    /// </summary>
    [HttpPost("delete")]
    [ProducesResponseType(typeof(DeleteAccountResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> DeleteAccount(
        [FromBody] DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Password is required" });
        }

        var result = await _deleteOwnAccountService.ExecuteAsync(User, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new DeleteAccountResponse { Success = true });
    }
}

public class DeleteAccountResponse
{
    public bool Success { get; set; }
}
