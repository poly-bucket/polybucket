using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Http
{
    [ApiController]
    [Route("api/system-settings/setup-status")]
    [AllowAnonymous]
    public class GetSetupStatusController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;

        public GetSetupStatusController(PolyBucketDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSetupStatus()
        {
            var settings = await _context.SystemSettings.ToListAsync();

            var adminSetupCompleted = settings.FirstOrDefault(s => s.Key == SystemSettingKeys.AdminSetupCompleted)?.Value == "true";
            var roleSetupCompleted = settings.FirstOrDefault(s => s.Key == SystemSettingKeys.RoleSetupCompleted)?.Value == "true";
            var moderationSetupCompleted = settings.FirstOrDefault(s => s.Key == SystemSettingKeys.ModerationSetupCompleted)?.Value == "true";

            var response = new SetupStatusResponse
            {
                IsAdminSetupComplete = adminSetupCompleted,
                IsRoleSetupComplete = roleSetupCompleted,
                IsModerationSetupComplete = moderationSetupCompleted
            };

            return Ok(response);
        }
    }

    public class SetupStatusResponse
    {
        public bool IsAdminSetupComplete { get; set; }
        public bool IsRoleSetupComplete { get; set; }
        public bool IsModerationSetupComplete { get; set; }
    }
} 