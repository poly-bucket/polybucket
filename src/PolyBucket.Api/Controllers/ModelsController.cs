using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Interfaces;
using PolyBucket.Core.Models;
using PolyBucket.Core.Enums;

namespace PolyBucket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelsController : ControllerBase
    {
        private readonly IModelService _modelService;
        private readonly IStorageService _storageService;
        private readonly ISystemSetupRepository _systemSetupRepository;

        public ModelsController(
            IModelService modelService, 
            IStorageService storageService,
            ISystemSetupRepository systemSetupRepository)
        {
            _modelService = modelService;
            _storageService = storageService;
            _systemSetupRepository = systemSetupRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModelWithDetails>>> GetAllModels([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var moderationRequired = await _systemSetupRepository.RequireUploadModerationAsync();
            var userId = User.Identity.IsAuthenticated ? Guid.Parse(User.FindFirst("sub")?.Value) : Guid.Empty;
            bool isModerator = User.Identity.IsAuthenticated && await _modelService.IsUserModeratorAsync(userId);
            
            // If moderation is required, only return approved models to normal users
            // Moderators can see all models
            var models = await _modelService.GetAllModelsAsync(page, pageSize);
            
            if (moderationRequired && !isModerator)
            {
                // Filter out non-approved models for non-moderators
                return Ok(models.Where(m => m.ModerationStatus == ModerationStatus.Approved));
            }
            
            return Ok(models);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ModelWithDetails>> GetModelById(Guid id)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var moderationRequired = await _systemSetupRepository.RequireUploadModerationAsync();
            var userId = User.Identity.IsAuthenticated ? Guid.Parse(User.FindFirst("sub")?.Value) : Guid.Empty;
            bool isModerator = User.Identity.IsAuthenticated && await _modelService.IsUserModeratorAsync(userId);
            bool isOwner = User.Identity.IsAuthenticated && model.UserId == userId;
            
            // If moderation is required, only return approved models to non-owners and non-moderators
            if (moderationRequired && model.ModerationStatus != ModerationStatus.Approved && !isOwner && !isModerator)
            {
                return NotFound("This model is not available.");
            }

            // Record view interaction if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                await _modelService.RecordModelInteractionAsync(id, userId, InteractionType.View);
            }

            return Ok(model);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ModelWithDetails>>> GetModelsByUser(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var moderationRequired = await _systemSetupRepository.RequireUploadModerationAsync();
            var currentUserId = User.Identity.IsAuthenticated ? Guid.Parse(User.FindFirst("sub")?.Value) : Guid.Empty;
            bool isModerator = User.Identity.IsAuthenticated && await _modelService.IsUserModeratorAsync(currentUserId);
            bool isOwner = User.Identity.IsAuthenticated && currentUserId == userId;
            
            var models = await _modelService.GetUserModelsAsync(userId, page, pageSize);
            
            // If moderation is required and the current user is not the owner or a moderator,
            // filter out non-approved models
            if (moderationRequired && !isOwner && !isModerator)
            {
                return Ok(models.Where(m => m.ModerationStatus == ModerationStatus.Approved));
            }
            
            return Ok(models);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ModelWithDetails>>> SearchModels([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var models = await _modelService.SearchModelsAsync(query, page, pageSize);
            return Ok(models);
        }

        [Authorize]
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<ModelUploadResult>> UploadModel([FromForm] ModelUploadRequest request)
        {
            if (request.ModelFile == null || request.ModelFile.Length == 0)
            {
                return BadRequest(new ModelUploadResult
                {
                    Success = false,
                    ErrorMessage = "No model file was uploaded."
                });
            }

            var moderationRequired = await _systemSetupRepository.RequireUploadModerationAsync();
            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            request.Tags ??= new List<string>();
            request.CategoryIds ??= new List<Guid>();

            var result = await _modelService.UploadModelAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Inform user if moderation is required
            if (moderationRequired)
            {
                result.Message = "Your model has been uploaded successfully and is pending moderation review.";
            }

            return CreatedAtAction(nameof(GetModelById), new { id = result.ModelId }, result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ModelUploadResult>> UpdateModel(Guid id, [FromForm] ModelUpdateRequest request)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            if (model.UserId != userId)
            {
                return Forbid();
            }

            var result = await _modelService.UpdateModelAsync(id, request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteModel(Guid id)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            if (model.UserId != userId)
            {
                return Forbid();
            }

            var result = await _modelService.DeleteModelAsync(id);
            if (!result)
            {
                return BadRequest("Failed to delete the model.");
            }

            return NoContent();
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadModel(Guid id)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var stream = await _modelService.DownloadModelFileAsync(id);
            if (stream == null)
            {
                return NotFound("Model file not found.");
            }

            // Record download interaction if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                var userId = Guid.Parse(User.FindFirst("sub")?.Value);
                await _modelService.RecordModelInteractionAsync(id, userId, InteractionType.Download);
            }

            // Get the main file from the model's files collection
            var mainFile = model.Files.FirstOrDefault(f => f.FileType == FileType.MainModel);
            if (mainFile == null)
            {
                return NotFound("Model file not found.");
            }

            // Return the file
            return File(stream, mainFile.MimeType ?? "application/octet-stream", mainFile.OriginalFileName);
        }

        [Authorize]
        [HttpPost("{id}/versions")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<ModelVersionUploadResult>> CreateModelVersion(Guid id, [FromForm] ModelVersionUploadRequest request)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            if (model.UserId != userId)
            {
                return Forbid();
            }

            var result = await _modelService.CreateModelVersionAsync(id, request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetModelById), new { id = result.ModelId }, result);
        }

        [HttpGet("{id}/versions")]
        public async Task<ActionResult<IEnumerable<ModelWithDetails>>> GetModelVersions(Guid id)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var versions = await _modelService.GetModelVersionsAsync(id);
            return Ok(versions);
        }

        [Authorize]
        [HttpPost("{id}/tags")]
        public async Task<ActionResult> AddTagToModel(Guid id, [FromBody] string tagName)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            if (model.UserId != userId)
            {
                return Forbid();
            }

            var result = await _modelService.AddTagToModelAsync(id, tagName);
            if (!result)
            {
                return BadRequest("Failed to add tag to model.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}/tags/{tagId}")]
        public async Task<ActionResult> RemoveTagFromModel(Guid id, Guid tagId)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            if (model.UserId != userId)
            {
                return Forbid();
            }

            var result = await _modelService.RemoveTagFromModelAsync(id, tagId);
            if (!result)
            {
                return BadRequest("Failed to remove tag from model.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost("{id}/categories/{categoryId}")]
        public async Task<ActionResult> AddCategoryToModel(Guid id, Guid categoryId)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            if (model.UserId != userId)
            {
                return Forbid();
            }

            var result = await _modelService.AddCategoryToModelAsync(id, categoryId);
            if (!result)
            {
                return BadRequest("Failed to add category to model.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}/categories/{categoryId}")]
        public async Task<ActionResult> RemoveCategoryFromModel(Guid id, Guid categoryId)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            if (model.UserId != userId)
            {
                return Forbid();
            }

            var result = await _modelService.RemoveCategoryFromModelAsync(id, categoryId);
            if (!result)
            {
                return BadRequest("Failed to remove category from model.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost("{id}/like")]
        public async Task<ActionResult> LikeModel(Guid id)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value);
            var result = await _modelService.RecordModelInteractionAsync(id, userId, InteractionType.Like);
            if (!result)
            {
                return BadRequest("Failed to like model.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost("{id}/collections/{collectionId}")]
        public async Task<ActionResult> AddModelToCollection(Guid id, Guid collectionId)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var result = await _modelService.AddModelToCollectionAsync(id, collectionId);
            if (!result)
            {
                return BadRequest("Failed to add model to collection.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}/collections/{collectionId}")]
        public async Task<ActionResult> RemoveModelFromCollection(Guid id, Guid collectionId)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var result = await _modelService.RemoveModelFromCollectionAsync(id, collectionId);
            if (!result)
            {
                return BadRequest("Failed to remove model from collection.");
            }

            return NoContent();
        }
    }
} 