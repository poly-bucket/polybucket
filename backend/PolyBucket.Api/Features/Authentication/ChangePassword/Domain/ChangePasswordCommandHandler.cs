using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Services;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Authentication.ChangePassword.Domain
{
    public class ChangePasswordCommandHandler(
        PolyBucketDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<ChangePasswordCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly ILogger<ChangePasswordCommandHandler> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "User not authenticated",
                        RequiresFirstTimeSetup = false
                    };
                }

                // Get user
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "User not found",
                        RequiresFirstTimeSetup = false
                    };
                }

                // Verify current password
                if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "Current password is incorrect",
                        RequiresFirstTimeSetup = false
                    };
                }

                // Hash new password
                var salt = _passwordHasher.GenerateSalt();
                var passwordHash = _passwordHasher.HashPassword(command.NewPassword, salt);

                // Update user password
                user.PasswordHash = passwordHash;
                user.Salt = salt;
                user.UpdatedAt = DateTime.UtcNow;
                user.RequiresPasswordChange = false;

                // Revoke all refresh tokens for security after password change
                var refreshTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                    .ToListAsync(cancellationToken);
                
                foreach (var token in refreshTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.ReasonRevoked = "Password changed";
                    token.RevokedByIp = "127.0.0.1"; // TODO: Get from request
                }

                // If this is the admin user and they haven't completed first-time setup, mark admin as configured
                if (user.Role?.Name == "Admin" && !user.HasCompletedFirstTimeSetup)
                {
                    var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                    if (systemSetup != null)
                    {
                        systemSetup.IsAdminConfigured = true;
                        systemSetup.UpdatedAt = DateTime.UtcNow;
                        systemSetup.UpdatedById = userId;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);

                return new ChangePasswordResponse
                {
                    Success = true,
                    Message = "Password changed successfully",
                    RequiresFirstTimeSetup = user.Role?.Name == "Admin" && !user.HasCompletedFirstTimeSetup
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change password for user");
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "Failed to change password. Please try again.",
                    RequiresFirstTimeSetup = false
                };
            }
        }

        public async Task<ChangePasswordResponse> SkipPasswordChange(CancellationToken cancellationToken)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "User not authenticated",
                        RequiresFirstTimeSetup = false
                    };
                }

                // Get user
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "User not found",
                        RequiresFirstTimeSetup = false
                    };
                }

                // Only allow skipping for admin users during first-time setup
                if (user.Role?.Name != "Admin" || user.HasCompletedFirstTimeSetup)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "Password change can only be skipped by admin users during first-time setup",
                        RequiresFirstTimeSetup = false
                    };
                }

                // Mark admin as configured without changing password
                var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                if (systemSetup != null)
                {
                    systemSetup.IsAdminConfigured = true;
                    systemSetup.UpdatedAt = DateTime.UtcNow;
                    systemSetup.UpdatedById = userId;
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password change skipped for admin user {UserId}", userId);

                return new ChangePasswordResponse
                {
                    Success = true,
                    Message = "Password change skipped successfully",
                    RequiresFirstTimeSetup = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to skip password change for user");
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "Failed to skip password change. Please try again.",
                    RequiresFirstTimeSetup = false
                };
            }
        }
    }
} 