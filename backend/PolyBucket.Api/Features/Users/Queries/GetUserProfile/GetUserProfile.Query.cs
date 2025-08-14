using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.Repository;
using PolyBucket.Api.Features.Users.Services;

namespace PolyBucket.Api.Features.Users.Queries.GetUserProfile
{
    public class GetUserProfileQuery
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
    }

    public class GetUserProfileResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string? Avatar { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Country { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? BannedAt { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BanExpiresAt { get; set; }
        
        // Statistics
        public int TotalModels { get; set; }
        public int TotalCollections { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDownloads { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowing { get; set; }
        
        // Privacy settings
        public bool IsProfilePublic { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowLastLogin { get; set; }
        public bool ShowStatistics { get; set; }
        
        // Social links (if implemented)
        public string? WebsiteUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? YouTubeUrl { get; set; }
    }

    public class PrivateProfileResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsProfilePublic { get; set; } = false;
        public string Message { get; set; } = "This profile is private";
    }

    public class GetUserProfileQueryHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly ILogger<GetUserProfileQueryHandler> _logger;

        public GetUserProfileQueryHandler(
            IUserRepository userRepository, 
            IUserStatisticsService userStatisticsService,
            ILogger<GetUserProfileQueryHandler> logger)
        {
            _userRepository = userRepository;
            _userStatisticsService = userStatisticsService;
            _logger = logger;
        }

        public async Task<object> Handle(GetUserProfileQuery query, CancellationToken cancellationToken = default)
        {
            User? user;
            
            if (!string.IsNullOrEmpty(query.Username))
            {
                user = await _userRepository.GetByUsernameAsync(query.Username);
            }
            else
            {
                user = await _userRepository.GetByIdAsync(query.Id);
            }

            if (user == null)
            {
                throw new KeyNotFoundException($"User not found");
            }

            // Check if profile is public
            if (!user.IsProfilePublic)
            {
                return new PrivateProfileResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    IsProfilePublic = false,
                    Message = "This profile is private"
                };
            }

            // Get user statistics
            var statistics = await _userStatisticsService.GetUserStatisticsAsync(user.Id, cancellationToken);
            
            return new GetUserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                Avatar = user.Avatar,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Country = user.Country,
                RoleName = user.Role?.Name ?? "Unknown",
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsBanned = user.IsBanned,
                BannedAt = user.BannedAt,
                BanReason = user.BanReason,
                BanExpiresAt = user.BanExpiresAt,
                
                // Real statistics from the service
                TotalModels = statistics.TotalModels,
                TotalCollections = statistics.TotalCollections,
                TotalLikes = statistics.TotalLikes,
                TotalDownloads = statistics.TotalDownloads,
                TotalFollowers = statistics.TotalFollowers,
                TotalFollowing = statistics.TotalFollowing,
                
                // Privacy settings from user model
                IsProfilePublic = user.IsProfilePublic,
                ShowEmail = user.ShowEmail,
                ShowLastLogin = user.ShowLastLogin,
                ShowStatistics = user.ShowStatistics,
                
                // Social links from user model
                WebsiteUrl = user.WebsiteUrl,
                TwitterUrl = user.TwitterUrl,
                InstagramUrl = user.InstagramUrl,
                YouTubeUrl = user.YouTubeUrl
            };
        }
    }
}
