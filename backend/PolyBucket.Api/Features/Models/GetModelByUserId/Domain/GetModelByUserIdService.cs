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

        /// <summary>
        /// Checks if a string is already a presigned URL or just an object key.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if it's already a URL, false if it's an object key</returns>
        private static bool IsPresignedUrl(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            
            // Check if it starts with http/https (presigned URL)
            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            // Check if it contains query parameters (presigned URL)
            if (value.Contains('?'))
            {
                return true;
            }
            
            // Check if it contains encoded URL parts (double-encoded presigned URL)
            if (value.Contains("%3A") || value.Contains("%2F") || value.Contains("%3F"))
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Safely generates a presigned URL only if the value is an object key.
        /// </summary>
        /// <param name="value">The value (object key or presigned URL)</param>
        /// <param name="expiry">Expiry time for the presigned URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The presigned URL (either existing or newly generated)</returns>
        private async Task<string> GetPresignedUrlSafelyAsync(string value, TimeSpan expiry, CancellationToken cancellationToken)
        {
            if (IsPresignedUrl(value))
            {
                return value;
            }
            
            return await _storageService.GetPresignedUrlAsync(value, expiry, cancellationToken);
        }

        public async Task<GetModelByUserIdResponse> GetModelsByUserIdAsync(Guid userId, GetModelByUserIdRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            // Handle public access (no authenticated user)
            if (user == null)
            {
                // For public access, only show public models, no private or deleted
                var (publicModels, publicTotalCount) = await _repository.GetModelsByUserIdAsync(userId, request.Page, request.Take, false, false, cancellationToken);

                // Generate fresh presigned URLs for thumbnail and file URLs (stored as object keys)
                foreach (var model in publicModels)
                {
                    if (!string.IsNullOrEmpty(model.ThumbnailUrl))
                    {
                        model.ThumbnailUrl = await GetPresignedUrlSafelyAsync(model.ThumbnailUrl, TimeSpan.FromHours(1), cancellationToken);
                    }

                    if (!string.IsNullOrEmpty(model.FileUrl))
                    {
                        model.FileUrl = await GetPresignedUrlSafelyAsync(model.FileUrl, TimeSpan.FromHours(1), cancellationToken);
                    }
                }

                var publicTotalPages = (int)Math.Ceiling((double)publicTotalCount / request.Take);

                return new GetModelByUserIdResponse
                {
                    Models = publicModels,
                    TotalCount = publicTotalCount,
                    Page = request.Page,
                    Take = request.Take,
                    TotalPages = publicTotalPages
                };
            }

            // Handle authenticated user access
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

            var (authModels, authTotalCount) = await _repository.GetModelsByUserIdAsync(userId, request.Page, request.Take, includePrivate, includeDeleted, cancellationToken);

            // Generate fresh presigned URLs for thumbnail and file URLs (stored as object keys)
            foreach (var model in authModels)
            {
                // Generate fresh presigned URLs for thumbnail and file URLs
                if (!string.IsNullOrEmpty(model.ThumbnailUrl))
                {
                    model.ThumbnailUrl = await GetPresignedUrlSafelyAsync(model.ThumbnailUrl, TimeSpan.FromHours(1), cancellationToken);
                }

                if (!string.IsNullOrEmpty(model.FileUrl))
                {
                    model.FileUrl = await GetPresignedUrlSafelyAsync(model.FileUrl, TimeSpan.FromHours(1), cancellationToken);
                }
            }

            var authTotalPages = (int)Math.Ceiling((double)authTotalCount / request.Take);

            return new GetModelByUserIdResponse
            {
                Models = authModels,
                TotalCount = authTotalCount,
                Page = request.Page,
                Take = request.Take,
                TotalPages = authTotalPages
            };
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
} 