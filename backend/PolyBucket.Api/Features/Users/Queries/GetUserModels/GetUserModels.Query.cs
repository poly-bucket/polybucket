using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;

namespace PolyBucket.Api.Features.Users.Queries.GetUserModels
{
    public class GetUserModelsQuery
    {
        public string Username { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludePrivate { get; set; } = false;
    }

    public class GetUserModelsResponse
    {
        public IEnumerable<UserModelDto> Models { get; set; } = new List<UserModelDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserModelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int Downloads { get; set; }
        public int Likes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? License { get; set; }
        public bool AIGenerated { get; set; }
        public bool WIP { get; set; }
        public bool NSFW { get; set; }
        public bool IsRemix { get; set; }
        public PrivacySettings Privacy { get; set; }
    }

    public class GetUserModelsQueryHandler
    {
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<GetUserModelsQueryHandler> _logger;

        public GetUserModelsQueryHandler(PolyBucketDbContext dbContext, ILogger<GetUserModelsQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<GetUserModelsResponse> Handle(GetUserModelsQuery query, CancellationToken cancellationToken = default)
        {
            // Get user ID from username
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == query.Username, cancellationToken);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with username {query.Username} not found");
            }

            // Check if profile is public
            if (!user.IsProfilePublic)
            {
                throw new UnauthorizedAccessException("User profile is private");
            }

            // Build query for models
            var modelsQuery = _dbContext.Models
                .Where(m => m.AuthorId == user.Id && m.DeletedAt == null);

            // Filter by privacy if not including private
            if (!query.IncludePrivate)
            {
                modelsQuery = modelsQuery.Where(m => m.Privacy == PrivacySettings.Public);
            }

            // Get total count
            var totalCount = await modelsQuery.CountAsync(cancellationToken);

            // Calculate pagination
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
            var skip = (query.Page - 1) * query.PageSize;

            // Get models with pagination
            var models = await modelsQuery
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(query.PageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Description,
                    m.ThumbnailUrl,
                    m.Downloads,
                    m.Likes,
                    m.CreatedAt,
                    m.UpdatedAt,
                    m.License,
                    m.AIGenerated,
                    m.WIP,
                    m.NSFW,
                    m.IsRemix,
                    m.Privacy
                })
                .ToListAsync(cancellationToken);

            // Map to DTOs with proper null handling
            var userModels = models.Select(m => new UserModelDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                ThumbnailUrl = m.ThumbnailUrl,
                Downloads = m.Downloads,
                Likes = m.Likes,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt ?? m.CreatedAt,
                License = m.License?.ToString(),
                AIGenerated = m.AIGenerated,
                WIP = m.WIP,
                NSFW = m.NSFW,
                IsRemix = m.IsRemix,
                Privacy = m.Privacy
            }).ToList();

            return new GetUserModelsResponse
            {
                Models = userModels,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };
        }
    }
}
