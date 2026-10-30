using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Domain;

namespace PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Http;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class TestFontAwesomeLicenseController(ITestFontAwesomeLicenseService testFontAwesomeLicenseService) : ControllerBase
{
    [HttpPost("fontawesome-settings/test-license")]
    [ProducesResponseType(200, Type = typeof(TestFontAwesomeLicenseResponseDto))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TestFontAwesomeLicenseResponseDto>> TestFontAwesomeLicense([FromBody] TestFontAwesomeLicenseRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.LicenseKey))
        {
            return BadRequest(new { message = "License key is required" });
        }
        var (isValid, message) = await testFontAwesomeLicenseService.TestFontAwesomeLicenseAsync(request.LicenseKey, cancellationToken);
        return Ok(new TestFontAwesomeLicenseResponseDto { IsValid = isValid, Message = message });
    }
}
