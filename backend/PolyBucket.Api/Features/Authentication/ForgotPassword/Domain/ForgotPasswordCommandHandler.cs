using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ForgotPassword.Domain
{
    public class ForgotPasswordCommandHandler(
        IAuthenticationRepository authRepository,
        ITokenService tokenService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;

        public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            // Check if user exists
            var user = await _authRepository.GetUserByEmailAsync(command.Email);
            if (user == null)
            {
                // Don't reveal if email exists or not for security reasons
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", command.Email);
                return;
            }

            // Generate reset token
            var resetToken = _tokenService.GeneratePasswordResetToken();
            var passwordResetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = resetToken,
                Email = command.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1" // TODO: Get from request
            };

            // Save reset token
            await _authRepository.CreatePasswordResetTokenAsync(passwordResetToken);

            // Send reset email
            var frontendUrl = _configuration["AppSettings:Frontend:BaseUrl"];
            var resetUrl = $"{frontendUrl}/reset-password";
            await _emailService.SendPasswordResetEmailAsync(command.Email, resetToken, resetUrl);

            _logger.LogInformation("Password reset email sent to: {Email}", command.Email);
        }
    }
} 