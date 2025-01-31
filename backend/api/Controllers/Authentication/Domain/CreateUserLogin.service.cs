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

public class CreateUserLoginService
{
    private readonly CreateUserLoginDataAccess _dataAccess;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateUserLoginService> _logger;
    private readonly string _jwtSecret;

    public CreateUserLoginService(CreateUserLoginDataAccess dataAccess, IPasswordHasher passwordHasher, ILogger<CreateUserLoginService> logger)
    {
        _dataAccess = dataAccess;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new ArgumentNullException("JWT_SECRET environment variable is not set");
    }

    public async Task<GetUserLoginResponse> CreateUserLoginAsync(string email, string password, string ipAddress, string userAgent)
    {
        var user = await _dataAccess.FindUserByEmailAsync(email);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {email} not found");
        }

        var loginSuccess = _passwordHasher.VerifyPassword(password, user.Salt, user.PasswordHash);

        var userLogin = new UserLogin
        {
            Email = user?.Username ?? email,
            Successful = loginSuccess,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };

        await _dataAccess.CreateLoginRecordAsync(userLogin);

        if (!loginSuccess)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }
    }

    public async Task<(string token, User user)> ExecuteAsync(string email, string password, string ipAddress, string userAgent)
    {
        var user = await _dataAccess.PersistUserLoginAttempt(email);

        var loginSuccess = user != null && _passwordHasher.VerifyPassword(password, user.Salt, user.PasswordHash);

        await _dataAccess.CreateLoginRecordAsync(userLogin);

        if (!loginSuccess)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        return (GenerateJwtToken(user!), user!);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("userId", user.Id.ToString())
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
