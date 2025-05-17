using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Core.Interfaces;
using Core.Models;

namespace PolyBucket.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/settings")]
    public class AdminSettingsController : ControllerBase
    {
        private readonly ISystemSetupRepository _systemSetupRepository;

        public AdminSettingsController(ISystemSetupRepository systemSetupRepository)
        {
            _systemSetupRepository = systemSetupRepository;
        }

        [HttpGet]
        public async Task<ActionResult<SystemSettings>> GetSystemSettings()
        {
            var setup = await _systemSetupRepository.GetSetupStatusAsync();
            
            return Ok(new SystemSettings
            {
                IsAdminConfigured = setup.IsAdminConfigured,
                IsRoleConfigured = setup.IsRoleConfigured,
                IsModerationConfigured = setup.IsModerationConfigured,
                RequireUploadModeration = setup.RequireUploadModeration,
                ModeratorRoles = setup.ModeratorRoles
            });
        }

        [HttpPut("moderation")]
        public async Task<ActionResult> UpdateModerationSettings([FromBody] ModerationSettings settings)
        {
            var setup = await _systemSetupRepository.GetSetupStatusAsync();
            setup.RequireUploadModeration = settings.RequireUploadModeration;
            setup.ModeratorRoles = settings.ModeratorRoles;
            setup.IsModerationConfigured = true;
            setup.UpdatedAt = DateTime.UtcNow;
            
            await _systemSetupRepository.UpdateSetupStatusAsync(setup);
            
            return NoContent();
        }
    }
} 