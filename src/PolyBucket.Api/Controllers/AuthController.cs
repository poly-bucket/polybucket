using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Core.Interfaces;
using PolyBucket.Core.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PolyBucket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using PolyBucket.Core.Exceptions;

namespace PolyBucket.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("check-first-run")]
        public async Task<IActionResult> CheckFirstRun()
        {
            try
            {
                var result = await _authService.IsFirstRunAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking first run");
                return StatusCode(500, "An error occurred while checking if this is the first run.");
            }
        }

        [HttpGet("setup-status")]
        public async Task<IActionResult> GetSetupStatus()
        {
            try
            {
                var result = await _authService.GetSetupStatusAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting setup status");
                return StatusCode(500, "An error occurred while getting setup status.");
            }
        }

        [HttpGet("is-admin-configured")]
        public async Task<IActionResult> IsAdminConfigured()
        {
            try
            {
                var result = await _authService.IsAdminConfiguredAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if admin is configured");
                return StatusCode(500, "An error occurred while checking if admin is configured.");
            }
        }

        [HttpGet("is-role-configured")]
        public async Task<IActionResult> IsRoleConfigured()
        {
            try
            {
                var result = await _authService.IsRoleConfiguredAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if roles are configured");
                return StatusCode(500, "An error occurred while checking if roles are configured.");
            }
        }

        [HttpPost("set-admin-configured")]
        public async Task<IActionResult> SetAdminConfigured([FromBody] bool isConfigured)
        {
            try
            {
                var result = await _authService.SetAdminConfiguredAsync(isConfigured);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting admin configuration status");
                return StatusCode(500, "An error occurred while setting admin configuration status.");
            }
        }

        [HttpPost("set-role-configured")]
        public async Task<IActionResult> SetRoleConfigured([FromBody] bool isConfigured)
        {
            try
            {
                var result = await _authService.SetRoleConfiguredAsync(isConfigured);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting role configuration status");
                return StatusCode(500, "An error occurred while setting role configuration status.");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var modelErrors = new Dictionary<string, List<string>>();
                    foreach (var pair in ModelState)
                    {
                        if (pair.Value.Errors.Count > 0)
                        {
                            modelErrors[pair.Key] = pair.Value.Errors
                                .Select(e => e.ErrorMessage)
                                .ToList();
                        }
                    }
                    
                    return BadRequest(new 
                    {
                        succeeded = false,
                        message = "Validation failed", 
                        validationErrors = modelErrors
                    });
                }

                // Try using the exception-based approach first if available
                try 
                {
                    // Use the exception-based method if it's been implemented
                    if (_authService is IAuthService authEx)
                    {
                        var result = await authEx.RegisterExAsync(request);
                        return Ok(new { succeeded = true, data = result });
                    }
                }
                catch (ValidationException ex)
                {
                    _logger.LogWarning("Validation errors during registration: {Message}", ex.Message);
                    return BadRequest(new
                    {
                        succeeded = false,
                        message = ex.Message,
                        validationErrors = ex.ValidationErrors
                    });
                }
                catch (ResourceExistsException ex)
                {
                    _logger.LogWarning("Resource already exists during registration: {Message}", ex.Message);
                    var errors = new Dictionary<string, List<string>>
                    {
                        { ex.ResourceName, new List<string> { ex.Message } }
                    };
                    
                    return BadRequest(new
                    {
                        succeeded = false,
                        message = ex.Message,
                        validationErrors = errors
                    });
                }
                catch (AuthException ex)
                {
                    // Don't fall through to legacy method if we have an actual error
                    return BadRequest(new
                    {
                        succeeded = false,
                        message = ex.Message
                    });
                }
                
                // Fall back to the legacy approach if needed
                var legacyResult = await _authService.RegisterAsync(request);
                
                if (!legacyResult.Succeeded)
                {
                    if (legacyResult.ValidationErrors != null && legacyResult.ValidationErrors.Count > 0)
                    {
                        // Return structured validation errors
                        return BadRequest(new
                        {
                            succeeded = false,
                            message = legacyResult.Message,
                            validationErrors = legacyResult.ValidationErrors
                        });
                    }
                    
                    // Return simple error message
                    return BadRequest(new 
                    { 
                        succeeded = false,
                        message = legacyResult.Message
                    });
                }
                
                return Ok(legacyResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration: {Message}", ex.Message);
                return StatusCode(500, new 
                { 
                    succeeded = false,
                    message = "An error occurred during registration.",
                    error = ex.StackTrace
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var modelErrors = new Dictionary<string, List<string>>();
                    foreach (var pair in ModelState)
                    {
                        if (pair.Value.Errors.Count > 0)
                        {
                            modelErrors[pair.Key] = pair.Value.Errors
                                .Select(e => e.ErrorMessage)
                                .ToList();
                        }
                    }
                    
                    return BadRequest(new 
                    {
                        succeeded = false,
                        message = "Validation failed", 
                        validationErrors = modelErrors
                    });
                }

                // Try using the exception-based approach first if available
                try
                {
                    // Use the exception-based method if it's been implemented
                    if (_authService is IAuthService authEx)
                    {
                        var result = await authEx.LoginExAsync(request);
                        return Ok(new { succeeded = true, data = result });
                    }
                }
                catch (ValidationException ex)
                {
                    _logger.LogWarning("Validation errors during login: {Message}", ex.Message);
                    return BadRequest(new
                    {
                        succeeded = false,
                        message = ex.Message,
                        validationErrors = ex.ValidationErrors
                    });
                }
                catch (InvalidCredentialsException ex)
                {
                    _logger.LogWarning("Invalid credentials during login: {Message}", ex.Message);
                    return Unauthorized(new
                    {
                        succeeded = false,
                        message = ex.Message
                    });
                }
                catch (AccountLockedException ex)
                {
                    _logger.LogWarning("Account locked during login: {Message}", ex.Message);
                    return Unauthorized(new
                    {
                        succeeded = false,
                        message = ex.Message,
                        lockoutEnd = ex.LockoutEnd
                    });
                }
                catch (EmailNotVerifiedException ex)
                {
                    _logger.LogWarning("Email not verified during login: {Message}", ex.Message);
                    return Unauthorized(new
                    {
                        succeeded = false,
                        message = ex.Message,
                        requiresEmailVerification = true
                    });
                }
                catch (AuthException ex)
                {
                    _logger.LogWarning("Auth exception during login: {Message}", ex.Message);
                    return Unauthorized(new
                    {
                        succeeded = false,
                        message = ex.Message
                    });
                }
                
                // Fall back to the legacy approach if needed
                var legacyResult = await _authService.LoginAsync(request);
                
                if (!legacyResult.Succeeded)
                {
                    return Unauthorized(new
                    {
                        succeeded = false,
                        message = legacyResult.Message
                    });
                }
                
                return Ok(legacyResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new
                {
                    succeeded = false,
                    message = "An error occurred during login."
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest("Refresh token is required");
                }

                var result = await _authService.RefreshTokenAsync(request);
                
                if (!result.Succeeded)
                {
                    return Unauthorized(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, "An error occurred while refreshing token.");
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required");
                }

                var result = await _authService.VerifyEmailAsync(token);
                
                if (!result.Succeeded)
                {
                    return BadRequest(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return StatusCode(500, "An error occurred while verifying email.");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.ForgotPasswordAsync(request);
                
                // Always return success even if the email doesn't exist for security reasons
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.ResetPasswordAsync(request);
                
                if (!result.Succeeded)
                {
                    return BadRequest(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, "An error occurred while resetting your password.");
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized("Invalid user");
                }

                var result = await _authService.ChangePasswordAsync(request, userGuid);
                
                if (!result.Succeeded)
                {
                    return BadRequest(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "An error occurred while changing your password.");
            }
        }
    }
} 