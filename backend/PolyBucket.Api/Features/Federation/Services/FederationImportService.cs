using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Services
{
    public class FederationImportService(
        IFederationRepository federationRepository,
        PolyBucketDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory) : IFederationImportService
    {
        private readonly IFederationRepository _federationRepository = federationRepository;
        private readonly PolyBucketDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<Model> ImportModelAsync(string instanceId, string remoteModelId)
        {
            var instance = await _federationRepository.GetFederatedInstanceAsync(Guid.Parse(instanceId));
            if (instance == null)
            {
                throw new InvalidOperationException($"Federated instance {instanceId} not found");
            }

            if (!instance.IsEnabled)
            {
                throw new InvalidOperationException($"Federated instance {instance.Name} is disabled");
            }

            var existingModel = await _context.Models
                .FirstOrDefaultAsync(m => m.RemoteModelId == remoteModelId && m.RemoteInstanceId == instanceId);

            if (existingModel != null)
            {
                throw new InvalidOperationException($"Model {remoteModelId} from instance {instanceId} has already been imported");
            }

            var remoteModel = await FetchRemoteModelMetadataAsync(instance.BaseUrl, remoteModelId);

            var localAuthor = await GetOrCreateFederatedUserAsync(remoteModel.Author, instanceId, instance.BaseUrl);

            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var userId = Guid.Parse(userIdClaim);
            var now = DateTime.UtcNow;

            var localModel = new Model
            {
                Id = Guid.NewGuid(),
                Name = remoteModel.Name,
                Description = remoteModel.Description ?? string.Empty,
                ThumbnailUrl = remoteModel.ThumbnailUrl,
                FileUrl = remoteModel.FileUrl,
                Downloads = 0,
                Likes = 0,
                License = ParseLicense(remoteModel.License),
                Privacy = PrivacySettings.Public,
                AIGenerated = remoteModel.AIGenerated,
                WIP = remoteModel.WIP,
                NSFW = remoteModel.NSFW,
                IsRemix = remoteModel.IsRemix,
                RemixUrl = remoteModel.RemixUrl,
                IsPublic = true,
                IsFeatured = false,
                AuthorId = localAuthor.Id,
                IsFederated = true,
                RemoteInstanceId = instanceId,
                RemoteModelId = remoteModelId,
                RemoteAuthorId = localAuthor.Id,
                LastFederationSync = now,
                CreatedById = userId,
                CreatedAt = now,
                UpdatedById = userId
            };

            _context.Models.Add(localModel);
            await _context.SaveChangesAsync();

            return localModel;
        }

        private async Task<RemoteModelDto> FetchRemoteModelMetadataAsync(string baseUrl, string remoteModelId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{baseUrl.TrimEnd('/')}/api/models/{remoteModelId}";

            try
            {
                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Failed to fetch model metadata from {url}: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<RemoteModelDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (model == null)
                {
                    throw new InvalidOperationException("Failed to deserialize remote model metadata");
                }

                return model;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to connect to remote instance: {ex.Message}", ex);
            }
        }

        private async Task<User> GetOrCreateFederatedUserAsync(RemoteUserDto remoteUser, string instanceId, string instanceBaseUrl)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.RemoteUserId == remoteUser.Id && u.RemoteInstanceId == instanceId);

            if (existing != null)
            {
                return existing;
            }

            var instanceDomain = new Uri(instanceBaseUrl).Host;
            
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var userId = Guid.Parse(userIdClaim);
            var now = DateTime.UtcNow;

            var federatedUser = new User
            {
                Id = Guid.NewGuid(),
                Username = $"{remoteUser.Username}@{instanceDomain}",
                Email = $"federated-{remoteUser.Id}@{instanceDomain}",
                FirstName = remoteUser.FirstName,
                LastName = remoteUser.LastName,
                Bio = remoteUser.Bio,
                Salt = string.Empty,
                PasswordHash = string.Empty,
                ProfilePictureUrl = remoteUser.ProfilePictureUrl,
                IsFederated = true,
                CanLogin = false,
                RemoteInstanceId = instanceId,
                RemoteUserId = remoteUser.Id,
                LastFederationSync = now,
                CreatedById = userId,
                CreatedAt = now,
                UpdatedById = userId
            };

            _context.Users.Add(federatedUser);
            await _context.SaveChangesAsync();

            return federatedUser;
        }

        private static LicenseTypes? ParseLicense(string? license)
        {
            if (string.IsNullOrWhiteSpace(license))
            {
                return null;
            }

            return license.ToLowerInvariant() switch
            {
                "ccby4" or "cc_by" or "ccby" => LicenseTypes.CCBy4,
                "ccbysa4" or "cc_by_sa" or "ccbysa" => LicenseTypes.CCBySA4,
                "ccbync4" or "cc_by_nc" or "ccbync" => LicenseTypes.CCByNC4,
                "ccbyncsa4" or "cc_by_nc_sa" or "ccbyncsa" => LicenseTypes.CCByNCSA4,
                "ccbynd4" or "cc_by_nd" or "ccbynd" => LicenseTypes.CCByND4,
                "ccbyncnd4" or "cc_by_nc_nd" or "ccbyncnd" => LicenseTypes.CCByNCND4,
                "mit" => LicenseTypes.MIT,
                "gplv3" or "gpl" => LicenseTypes.GPLv3,
                "apache2" or "apache" => LicenseTypes.Apache2,
                "bsd" => LicenseTypes.BSD,
                _ => null
            };
        }
    }

    public class RemoteModelDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? FileUrl { get; set; }
        public string? License { get; set; }
        public bool AIGenerated { get; set; }
        public bool WIP { get; set; }
        public bool NSFW { get; set; }
        public bool IsRemix { get; set; }
        public string? RemixUrl { get; set; }
        public RemoteUserDto Author { get; set; } = new();
        public List<RemoteFileDto> Files { get; set; } = new();
    }

    public class RemoteUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class RemoteFileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}

