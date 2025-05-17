using Core.Models.Auth;
using Microsoft.AspNetCore.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Authentication.ResetPassword.Persistance
{
    public class ResetPasswordService
    {
        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await ResetPasswordExAsync(request, cancellationToken);
                return ServiceResponse<bool>.Success(result);
            }
            catch (ValidationException ex)
            {
                return ServiceResponse<bool>.ValidationFailure(ex.Message, ex.ValidationErrors);
            }
            catch (AuthException ex)
            {
                return ServiceResponse<bool>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password: {Message} {InnerException}", ex.Message, ex.InnerException?.Message);
                throw new AuthException($"Error resetting password: {ex.Message} Inner: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<bool> ResetPasswordExAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            // Validate request
            var validationErrors = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                AddValidationError(validationErrors, "Token", "Token is required");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                AddValidationError(validationErrors, "Password", "Password is required");
            }
            else if (request.Password.Length < 6)
            {
                AddValidationError(validationErrors, "Password", "Password must be at least 6 characters");
            }

            if (validationErrors.Count > 0)
            {
                throw new ValidationException("Please correct the validation errors", validationErrors);
            }

            // Get user by password reset token
            var user = await _userRepository.GetByIdAsync(Guid.Parse(request.Token));
            if (user == null)
            {
                throw new AuthException("Invalid reset token");
            }

            // Check if token is expired
            if (!user.PasswordResetTokenExpiry.HasValue || user.PasswordResetTokenExpiry.Value <= DateTime.UtcNow)
            {
                throw new AuthException("Reset token has expired");
            }

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.UpdateAsync(user);

            return true;
        }
    }
}