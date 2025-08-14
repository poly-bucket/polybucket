using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Repository;
using PolyBucket.Api.Features.Models.GetModelById.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using PolyBucket.Api.Common.Storage;

namespace PolyBucket.Api.Features.Models.GetModelById.Domain
{
    public class GetModelByIdQueryHandler : IRequestHandler<GetModelByIdQuery, GetModelByIdResponse>
    {
        private readonly IModelsRepository _repository;
        private readonly ILogger<GetModelByIdQueryHandler> _logger;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStorageService _storageService;

        public GetModelByIdQueryHandler(
            IModelsRepository repository, 
            ILogger<GetModelByIdQueryHandler> logger,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            IStorageService storageService)
        {
            _repository = repository;
            _logger = logger;
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
            _storageService = storageService;
        }

        public async Task<GetModelByIdResponse> Handle(GetModelByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("GetModelByIdQueryHandler.Handle called with ID: {Id}", request.Id);
                
                var model = await _repository.GetModelByIdAsync(request.Id);
            
                if (model == null)
                {
                    throw new KeyNotFoundException($"Model with ID {request.Id} not found");
                }

                _logger.LogDebug("Model found: {ModelName}", model.Name);

                // Check privacy-based authorization
                if (!await CanUserAccessModel(model))
                {
                    throw new UnauthorizedAccessException("You do not have permission to access this model");
                }

                _logger.LogDebug("User has permission to access model");

                // Generate fresh presigned URLs for the model files (stored as object keys)
                if (model.FileUrl != null)
                {
                    _logger.LogDebug("Generating presigned URL for FileUrl: {FileUrl}", model.FileUrl);
                    model.FileUrl = await _storageService.GetPresignedUrlAsync(model.FileUrl, TimeSpan.FromHours(1), cancellationToken);
                    _logger.LogDebug("Generated presigned URL for FileUrl: {FileUrl}", model.FileUrl);
                }
                if (model.ThumbnailUrl != null)
                {
                    _logger.LogDebug("Generating presigned URL for ThumbnailUrl: {ThumbnailUrl}", model.ThumbnailUrl);
                    model.ThumbnailUrl = await _storageService.GetPresignedUrlAsync(model.ThumbnailUrl, TimeSpan.FromHours(1), cancellationToken);
                    _logger.LogDebug("Generated presigned URL for ThumbnailUrl: {ThumbnailUrl}", model.ThumbnailUrl);
                }

                // Generate fresh presigned URLs for all files
                if (model.Files != null)
                {
                    _logger.LogDebug("Generating presigned URLs for {FileCount} files", model.Files.Count);
                    foreach (var file in model.Files)
                    {
                        if (!string.IsNullOrEmpty(file.Path))
                        {
                            _logger.LogDebug("Generating presigned URL for file {FileName} with path {Path}", file.Name, file.Path);
                            file.Path = await _storageService.GetPresignedUrlAsync(file.Path, TimeSpan.FromHours(1), cancellationToken);
                            _logger.LogDebug("Generated presigned URL for file {FileName}: {Path}", file.Name, file.Path);
                        }
                    }
                }

                _logger.LogDebug("Returning model response");

                return new GetModelByIdResponse
                {
                    Model = model
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error getting model with ID {Id}", request.Id);
                throw;
            }
        }

        private async Task<bool> CanUserAccessModel(Model model)
        {
            // Public models are accessible to everyone
            if (model.Privacy == PrivacySettings.Public)
            {
                return true;
            }

            // Get current user ID
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                // No authenticated user - can only access public models
                return false;
            }

            // Model owner can always access their own models
            if (model.AuthorId == currentUserId)
            {
                return true;
            }

            // Check if user has admin or moderator privileges
            var isAdmin = await _permissionService.IsAdminAsync(currentUserId);
            var userRole = await _permissionService.GetUserRoleAsync(currentUserId);
            var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
            
            if (isAdmin || isModerator)
            {
                return true;
            }

            // For Private models, only owner and admins/moderators can access
            if (model.Privacy == PrivacySettings.Private)
            {
                return false;
            }

            // For Unlisted models, anyone with the link can access (as per user requirements)
            // The user who uploaded can always see it, and moderators/admins can see it too
            // This is already handled above, so if we reach here, it means the user is authenticated
            // and has the link, so they can access it
            if (model.Privacy == PrivacySettings.Unlisted)
            {
                return true;
            }

            return false;
        }
    }
} 