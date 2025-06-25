using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.Repository;

namespace PolyBucket.Api.Features.Authentication.Commands
{
    public class LoginCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(IUserRepository userRepository, IConfiguration configuration, ILogger<LoginCommandHandler> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginCommandResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            {
                // Log failed login attempt
                // You might want to get the IP address from the HttpContext here if needed
                await LogLoginAttempt(command.Email, false);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Log successful login attempt
            await LogLoginAttempt(command.Email, true, user.Id);
            
            var token = GenerateJwtToken(user);
            return new LoginCommandResponse { Token = token };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Security:JwtSecret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("role", user.Role.ToString()) 
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Security:JwtIssuer"],
                Audience = _configuration["Security:JwtAudience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task LogLoginAttempt(string email, bool success, Guid? userId = null)
        {
            // This is a simplified logging. In a real app, you would inject the DbContext
            // or a specific repository for logging.
            _logger.LogInformation("Login attempt for {Email}. Success: {Success}", email, success);

            // In a real application, you would do this:
            // var login = new UserLogin { ... };
            // await _context.UserLogins.AddAsync(login);
            // await _context.SaveChangesAsync();
        }
    }
} 