using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Linq;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Enums;

namespace PolyBucket.Api.Features.SystemSettings.Http
{
    [ApiController]
    [Route("api/system-settings/admin-setup")]
    [AllowAnonymous]
    public class AdminSetupController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AdminSetupController(PolyBucketDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public async Task<IActionResult> AdminSetup(AdminSetupRequest request)
        {
            var adminSetupCompletedSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AdminSetupCompleted);

            if (adminSetupCompletedSetting != null && adminSetupCompletedSetting.Value == "true")
            {
                return BadRequest("Admin setup has already been completed.");
            }
            
            if (await _context.Users.AnyAsync(u => u.Role == UserRole.Admin))
            {
                return BadRequest("An admin user already exists.");
            }

            var salt = _passwordHasher.GenerateSalt();
            var hashedPassword = _passwordHasher.HashPassword(request.Password, salt);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = UserRole.Admin,
                CreatedAt = System.DateTime.UtcNow,
                Salt = salt,
                PasswordHash = hashedPassword
            };
            
            _context.Users.Add(user);
            
            var userLogin = new UserLogin
            {
                Email = user.Email,
                Successful = true,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                CreatedAt = System.DateTime.UtcNow,
                UserId = user.Id,
                User = user
            };
            _context.UserLogins.Add(userLogin);

            if (adminSetupCompletedSetting == null)
            {
                _context.SystemSettings.Add(new SystemSetting { Key = SystemSettingKeys.AdminSetupCompleted, Value = "true" });
            }
            else
            {
                adminSetupCompletedSetting.Value = "true";
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class AdminSetupRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
} 