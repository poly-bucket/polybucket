using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Services;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.Http
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class FontAwesomeSettingsController : ControllerBase
    {
        private readonly IFontAwesomeSettingsService _fontAwesomeSettingsService;
        private readonly ILogger<FontAwesomeSettingsController> _logger;

        public FontAwesomeSettingsController(
            IFontAwesomeSettingsService fontAwesomeSettingsService,
            ILogger<FontAwesomeSettingsController> logger)
        {
            _fontAwesomeSettingsService = fontAwesomeSettingsService;
            _logger = logger;
        }

        /// <summary>
        /// Returns Font Awesome integration settings without exposing the stored license key.
        /// </summary>
        [HttpGet("fontawesome-settings")]
        [ProducesResponseType(200, Type = typeof(GetFontAwesomeSettingsResponseDto))]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetFontAwesomeSettingsResponseDto>> GetSettings()
        {
            try
            {
                var settings = await _fontAwesomeSettingsService.GetSettingsAsync();
                return Ok(GetFontAwesomeSettingsResponseDto.FromEntity(settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting FontAwesome settings");
                return StatusCode(500, new { message = "An error occurred while retrieving FontAwesome settings" });
            }
        }

        [HttpPost("fontawesome-settings")]
        [ProducesResponseType(200, Type = typeof(GetFontAwesomeSettingsResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetFontAwesomeSettingsResponseDto>> UpdateSettings([FromBody] UpdateFontAwesomeSettingsRequestDto settings)
        {
            try
            {
                if (settings == null)
                {
                    return BadRequest(new { message = "Settings data is required" });
                }

                var current = await _fontAwesomeSettingsService.GetSettingsAsync();
                var entity = settings.ToEntity(current.Id);
                if (string.IsNullOrEmpty(settings.ProLicenseKey))
                {
                    entity.ProLicenseKey = current.ProLicenseKey;
                }
                var updatedSettings = await _fontAwesomeSettingsService.UpdateSettingsAsync(entity);
                return Ok(GetFontAwesomeSettingsResponseDto.FromEntity(updatedSettings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FontAwesome settings");
                return StatusCode(500, new { message = "An error occurred while updating FontAwesome settings" });
            }
        }

        [HttpPost("fontawesome-settings/test-license")]
        [ProducesResponseType(200, Type = typeof(TestLicenseResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TestLicenseResponse>> TestLicense([FromBody] TestLicenseRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LicenseKey))
                {
                    return BadRequest(new { message = "License key is required" });
                }

                var isValid = await _fontAwesomeSettingsService.TestLicenseKeyAsync(request.LicenseKey);
                
                return Ok(new TestLicenseResponse
                {
                    IsValid = isValid,
                    Message = isValid ? "License key is valid" : "Invalid license key"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing FontAwesome license key");
                return StatusCode(500, new { message = "An error occurred while testing the license key" });
            }
        }

        [HttpGet("fontawesome-settings/pro-enabled")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> IsProEnabled()
        {
            try
            {
                var isEnabled = await _fontAwesomeSettingsService.IsProEnabledAsync();
                return Ok(isEnabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if FontAwesome Pro is enabled");
                return StatusCode(500, new { message = "An error occurred while checking FontAwesome Pro status" });
            }
        }
    }

    public class TestLicenseRequest
    {
        public string LicenseKey { get; set; } = string.Empty;
    }

    public class TestLicenseResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
