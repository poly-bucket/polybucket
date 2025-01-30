using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Services;
using Core.Models.Users;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Conductors.Login;

public interface ILoginConductor
{
    Task<(string token, User user)> LoginAsync(string email, string password, string ipAddress, string userAgent);
}

public class LoginConductor : ILoginConductor
{
    private readonly Context _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly string _jwtSecret;

    public LoginConductor(Context context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new ArgumentNullException("JWT_SECRET environment variable is not set");
    }

    public async Task<(string token, User user)> LoginAsync(string email, string password, string ipAddress, string userAgent)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        var loginSuccess = user != null && _passwordHasher.VerifyPassword(password, user.Salt, user.PasswordHash);

        // Create login record
        var userLogin = new UserLogin
        {
            Email = user?.Username ?? email,
            Successful = loginSuccess,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };

        await _context.UserLogins.AddAsync(userLogin);
        await _context.SaveChangesAsync();

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