using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Interfaces;
using PolyBucket.Core.Models;

namespace PolyBucket.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/moderation/models")]
    public class ModelModerationController : ControllerBase
    {
        private readonly IModelService _modelService;
        private readonly ISystemSetupRepository _systemSetupRepository;

        public ModelModerationController(IModelService modelService, ISystemSetupRepository systemSetupRepository)
        {
            _modelService = modelService;
            _systemSetupRepository = systemSetupRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model>>> GetModelsAwaitingModeration([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            // Check if user is a moderator
            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            bool isModerator = await _modelService.IsUserModeratorAsync(userId);
            
            if (!isModerator)
            {
                return Forbid();
            }
            
            var models = await _modelService.GetModelsAwaitingModerationAsync(page, pageSize);
            return Ok(models);
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult> ApproveModel(Guid id)
        {
            // Check if moderation is required
            var moderationRequired = await _systemSetupRepository.RequireUploadModerationAsync();
            if (!moderationRequired)
            {
                return BadRequest("Moderation is not currently required.");
            }
            
            // Check if user is a moderator
            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            bool isModerator = await _modelService.IsUserModeratorAsync(userId);
            
            if (!isModerator)
            {
                return Forbid();
            }

            var success = await _modelService.ApproveModelAsync(id, userId);
            
            if (!success)
            {
                return BadRequest("Failed to approve model.");
            }
            
            return NoContent();
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult> RejectModel(Guid id, [FromBody] ModelRejectRequest request)
        {
            // Check if moderation is required
            var moderationRequired = await _systemSetupRepository.RequireUploadModerationAsync();
            if (!moderationRequired)
            {
                return BadRequest("Moderation is not currently required.");
            }
            
            // Check if user is a moderator
            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            bool isModerator = await _modelService.IsUserModeratorAsync(userId);
            
            if (!isModerator)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest("A reason for rejection is required.");
            }

            var success = await _modelService.RejectModelAsync(id, userId, request.Reason);
            
            if (!success)
            {
                return BadRequest("Failed to reject model.");
            }
            
            return NoContent();
        }
        
        [HttpGet("settings")]
        public async Task<ActionResult<ModerationSettings>> GetModerationSettings()
        {
            // Check if user is a moderator
            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            bool isModerator = await _modelService.IsUserModeratorAsync(userId);
            
            if (!isModerator)
            {
                return Forbid();
            }
            
            var setup = await _systemSetupRepository.GetSetupStatusAsync();
            
            return Ok(new ModerationSettings
            {
                RequireUploadModeration = setup.RequireUploadModeration,
                ModeratorRoles = setup.ModeratorRoles
            });
        }
        
        [HttpPut("settings")]
        public async Task<ActionResult> UpdateModerationSettings([FromBody] ModerationSettings settings)
        {
            // Check if user is a moderator with admin rights
            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            bool isModerator = await _modelService.IsUserModeratorAsync(userId);
            bool isAdmin = User.IsInRole("Admin");
            
            if (!isModerator || !isAdmin)
            {
                return Forbid();
            }
            
            var setup = await _systemSetupRepository.GetSetupStatusAsync();
            setup.RequireUploadModeration = settings.RequireUploadModeration;
            setup.ModeratorRoles = settings.ModeratorRoles;
            
            await _systemSetupRepository.UpdateSetupStatusAsync(setup);
            
            return NoContent();
        }
    }
} 