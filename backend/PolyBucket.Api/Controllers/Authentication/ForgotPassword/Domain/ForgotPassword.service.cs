using Core.Configuration;
using Core.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Authentication.ForgotPassword.Domain
{
    public class ForgotPasswordService
    {
        public async Task<ServiceResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await ForgotPasswordExAsync(request, cancellationToken);
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
                _logger.LogError(ex, "Error processing forgot password request: {Message} {InnerException}", ex.Message, ex.InnerException?.Message);
                throw new AuthException($"Error processing forgot password request: {ex.Message} Inner: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<bool> ForgotPasswordExAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ValidationException("Email is required", new Dictionary<string, List<string>>
                {
                    { "Email", new List<string> { "Email is required" } }
                });
            }

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the email doesn't exist
                return true;
            }

            // Generate password reset token
            user.PasswordResetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24); // Use 24 hours as default

            await _userRepository.UpdateAsync(user);

            // Send password reset email
            var resetLink = $"{_appSettings.Frontend.BaseUrl}/reset-password?token={user.PasswordResetToken}";
            var emailSubject = "Reset your password";
            var emailBody = $"Please click the following link to reset your password: {resetLink}";

            await _emailService.SendAsync(user.Email, emailSubject, emailBody, cancellationToken);

            return true;
        }
    }
}