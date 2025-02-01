using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Services;
using Core.Models.Users;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Api.Controllers.Authentication.Persistance;

namespace Api.Controllers.Authentication.Domain;

public class CreateUserLoginService(CreateUserLoginDataAccess dataAccess, IPasswordHasher passwordHasher, ILogger<CreateUserLoginService> logger)
{
    private readonly CreateUserLoginDataAccess _dataAccess = dataAccess;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ILogger<CreateUserLoginService> _logger = logger;
    private readonly string _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new ArgumentNullException("JWT_SECRET environment variable is not set");

    public async Task<CreateUserLoginResponse> CreateUserLoginAsync(string email, string password, string ipAddress, string userAgent)
    {
        // Find what user we're trying to login
        _logger.LogDebug($"Attempting to login user with email {email}");
        var user = await _dataAccess.FindUserByEmailAsync(email);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {email} not found");
        }

        // Lets check if the passwords match.
        var loginSuccess = _passwordHasher.VerifyPassword(password, user.Salt, user.PasswordHash);

        var userLogin = new UserLogin
        {
            Email = user?.Email ?? email,
            Successful = loginSuccess,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        await _dataAccess.CreateLoginRecordAsync(userLogin);

        if (loginSuccess && user != null)
        {
            //TODO: Re-salt the user's password and save the new salt.

            return new CreateUserLoginResponse
            {
                Token = GenerateJwtToken(user, userLogin.UserAgent),
            };
        }

        throw new UnauthorizedAccessException("Invalid email or password");
    }

    private string GenerateJwtToken(User user, string userAgent)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("sub", user.Id.ToString()),
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