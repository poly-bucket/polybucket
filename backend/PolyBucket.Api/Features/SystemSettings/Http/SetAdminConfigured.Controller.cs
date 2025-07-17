using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Http
{
    [ApiController]
    [Route("api/system-settings")]
    [AllowAnonymous]
    public class SetAdminConfiguredController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;

        public SetAdminConfiguredController(PolyBucketDbContext context)
        {
            _context = context;
        }

        [HttpPost("set-admin-configured")]
        public async Task<IActionResult> SetAdminConfigured([FromBody] bool isConfigured)
        {
            var adminSetupCompletedSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.AdminSetupCompleted);

            if (adminSetupCompletedSetting == null)
            {
                _context.SystemSettings.Add(new SystemSetting 
                { 
                    Key = SystemSettingKeys.AdminSetupCompleted, 
                    Value = isConfigured.ToString().ToLower() 
                });
            }
            else
            {
                adminSetupCompletedSetting.Value = isConfigured.ToString().ToLower();
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, isConfigured });
        }
    }
} 