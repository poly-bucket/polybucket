using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PolyBucket.Api.Features.Authentication.Domain
{
    public class LoginService
    {
        // private readonly IAuthenticationRepository _authRepository;
        // private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<LoginService> _logger;
        private readonly string _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default-secret";

        public LoginService(ILogger<LoginService> logger) //, IAuthenticationRepository authRepository, IPasswordHasher passwordHasher)
        {
            _logger = logger;
            // _authRepository = authRepository;
            // _passwordHasher = passwordHasher;
        }

        public async Task<LoginResponse> LoginAsync(string email, string password, string ipAddress, string? userAgent)
        {
            // ... (logic will be uncommented later)
            await Task.CompletedTask;
            return new LoginResponse { Token = "dummy-token-from-service" };
        }

        private string GenerateJwtToken(object user, string userAgent) // User user
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "username"), // user.Username
                    new Claim(ClaimTypes.Role, "User"), // user.Role.ToString()
                    new Claim("sub", "userid"), // user.Id.ToString()
                    new Claim("agent", userAgent)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
} 