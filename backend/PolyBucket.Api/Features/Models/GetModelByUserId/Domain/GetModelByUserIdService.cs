using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.GetModelByUserId.Http;
using PolyBucket.Api.Features.Models.GetModelByUserId.Repository;
using PolyBucket.Api.Common.Storage;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelByUserId.Domain
{
    public class GetModelByUserIdService : IGetModelByUserIdService
    {
        private readonly IGetModelByUserIdRepository _repository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<GetModelByUserIdService> _logger;
        private readonly IStorageService _storageService;

        public GetModelByUserIdService(IGetModelByUserIdRepository repository, IPermissionService permissionService, ILogger<GetModelByUserIdService> logger, IStorageService storageService)
        {
            _repository = repository;
            _permissionService = permissionService;
            _logger = logger;
            _storageService = storageService;
        }

        public async Task<GetModelByUserIdResponse> GetModelsByUserIdAsync(Guid userId, GetModelByUserIdRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdClaim, out var currentUserId))
            {
                throw new ValidationException("Invalid authentication token");
            }

            // Check if user is requesting their own models or has permission to view other users' models
            var isOwnModels = currentUserId == userId;
            var hasViewPrivatePermission = await _permissionService.HasPermissionAsync(currentUserId, PermissionConstants.MODEL_VIEW_PRIVATE);

            if (!isOwnModels && !hasViewPrivatePermission)
            {
                throw new ValidationException("You do not have permission to view models from other users");
            }

            // If not own models and no special permission, only show public models
            var includePrivate = isOwnModels && request.IncludePrivate;
            var includeDeleted = isOwnModels && request.IncludeDeleted;

            var (models, totalCount) = await _repository.GetModelsByUserIdAsync(userId, request.Page, request.Take, includePrivate, includeDeleted, cancellationToken);

            // Generate fresh presigned URLs for thumbnail and file URLs
            foreach (var model in models)
            {
                // Generate fresh presigned URLs for thumbnail and file URLs
                if (!string.IsNullOrEmpty(model.ThumbnailUrl))
                {
                    // Check if it's already a presigned URL (contains query parameters)
                    if (!model.ThumbnailUrl.Contains("?"))
                    {
                        model.ThumbnailUrl = await _storageService.GetPresignedUrlAsync(model.ThumbnailUrl, TimeSpan.FromHours(1), cancellationToken);
                    }
                }

                if (!string.IsNullOrEmpty(model.FileUrl))
                {
                    // Check if it's already a presigned URL (contains query parameters)
                    if (!model.FileUrl.Contains("?"))
                    {
                        model.FileUrl = await _storageService.GetPresignedUrlAsync(model.FileUrl, TimeSpan.FromHours(1), cancellationToken);
                    }
                }
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

            return new GetModelByUserIdResponse
            {
                Models = models,
                TotalCount = totalCount,
                Page = request.Page,
                Take = request.Take,
                TotalPages = totalPages
            };
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
} 