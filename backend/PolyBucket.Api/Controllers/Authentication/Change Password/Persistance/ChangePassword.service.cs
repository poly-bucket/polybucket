using Api.Controllers.Authentication.Change_Password.Domain;
using Core.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Authentication.Change_Password.Persistance
{
    public class ChangePasswordService
    {
        public async Task<ServiceResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await ChangePasswordExAsync(request, userId, cancellationToken);
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
                _logger.LogError(ex, "Error changing password: {Message} {InnerException}", ex.Message, ex.InnerException?.Message);
                throw new AuthException($"Error changing password: {ex.Message} Inner: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<bool> ChangePasswordExAsync(ChangePasswordRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            // Validate request
            var validationErrors = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                AddValidationError(validationErrors, "CurrentPassword", "Current password is required");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                AddValidationError(validationErrors, "NewPassword", "New password is required");
            }
            else if (request.NewPassword.Length < 6)
            {
                AddValidationError(validationErrors, "NewPassword", "New password must be at least 6 characters");
            }

            if (validationErrors.Count > 0)
            {
                throw new ValidationException("Please correct the validation errors", validationErrors);
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new AuthException("User not found");
            }

            // Verify current password
            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                throw new AuthException("Current password is incorrect");
            }

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.UpdateAsync(user);

            return true;
        }
    }
}