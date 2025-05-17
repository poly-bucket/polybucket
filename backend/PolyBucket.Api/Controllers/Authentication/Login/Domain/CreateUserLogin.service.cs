using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Core.Interfaces;
using Api.Controllers.Authentication.Login.Persistance;

namespace Api.Controllers.Authentication.Login.Domain;

public class CreateUserLoginService(
    CreateUserLoginDataAccess dataAccess,
    ILogger<CreateUserLoginService> logger,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
{
    private readonly CreateUserLoginDataAccess _dataAccess = dataAccess;
    private readonly ILogger<CreateUserLoginService> _logger = logger;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly string _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new ArgumentNullException("JWT_SECRET environment variable is not set");

    public async Task<CreateUserLoginResponse> CreateUserLoginAsync(CreateUserLoginRequest userLoginRequest, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug($"Attempting to login user with email {userLoginRequest.EmailOrUsername}");

        // Validate the request
        var validationErrors = ValidateRequest(userLoginRequest, ipAddress);

        if (validationErrors.Count > 0)
        {
            throw new ArgumentException("Invalid request", nameof(userLoginRequest));
        }

        var user = await _dataAccess.FindUserByEmailAsync(userLoginRequest.EmailOrUsername);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {userLoginRequest.EmailOrUsername} not found");
        }

        // Lets check if the passwords match.
        var loginSuccess = _passwordHasher.VerifyPassword(userLoginRequest.Password, user.PasswordHash);

        var userLogin = new UserLogin
        {
            Email = user?.Email ?? userLoginRequest.EmailOrUsername,
            Successful = loginSuccess,
            IpAddress = ipAddress,
            UserAgent = userLoginRequest.UserAgent,
            CreatedAt = DateTime.UtcNow
        };

        await _dataAccess.CreateLoginRecordAsync(userLogin);

        if (loginSuccess && user != null)
        {
            //TODO: Re-salt the user's password and save the new salt.
            return new CreateUserLoginResponse
            {
                AccessToken = GenerateJwtToken(user, userLogin.UserAgent),
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

    private Dictionary<string, List<string>> ValidateRequest(CreateUserLoginRequest createUserLogin, string ipAddress)
    {
        var errors = new Dictionary<string, List<string>>();
        if (string.IsNullOrWhiteSpace(createUserLogin.EmailOrUsername))
        {
            errors.Add("EmailOrUsername", new List<string> { "Email or username is required" });
        }
        if (string.IsNullOrWhiteSpace(createUserLogin.Password))
        {
            errors.Add("Password", new List<string> { "Password is required" });
        }

        return errors;
    }
}