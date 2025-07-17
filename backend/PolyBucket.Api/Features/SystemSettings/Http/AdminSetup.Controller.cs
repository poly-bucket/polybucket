using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Linq;
using System.Threading.Tasks;

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
            
            // Check if there's already an admin user
            var existingAdmin = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Role != null && u.Role.Name == "Admin");
            
            if (existingAdmin != null)
            {
                // If we're not replacing the default admin, return error
                if (!request.ReplaceDefaultAdmin)
                {
                    return BadRequest("An admin user already exists.");
                }
                
                // If we are replacing, soft delete the existing admin
                existingAdmin.DeletedAt = System.DateTime.UtcNow;
            }

            var salt = _passwordHasher.GenerateSalt();
            var hashedPassword = _passwordHasher.HashPassword(request.Password, salt);

            // Find the Admin role
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                return BadRequest("Admin role not found. Please ensure roles are properly configured.");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                RoleId = adminRole.Id,
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
        public bool ReplaceDefaultAdmin { get; set; } = false;
    }
} 