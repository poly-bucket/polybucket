using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using PolyBucket.Api.Features.Federation.Repository;

namespace PolyBucket.Api.Features.Federation.Services
{
    public class FederationService(
        PolyBucketDbContext context,
        IFederationCryptographyService cryptographyService,
        IPermissionService permissionService,
        IFederationRepository federationRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<FederationService> logger) : IFederationService
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IFederationCryptographyService _cryptographyService = cryptographyService;
        private readonly IPermissionService _permissionService = permissionService;
        private readonly IFederationRepository _federationRepository = federationRepository;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<FederationService> _logger = logger;

        public async Task<FederationSettings?> GetFederationSettingsAsync()
        {
            return await _federationRepository.GetFederationSettingsAsync();
        }

        public async Task<FederationSettings> UpdateFederationSettingsAsync(FederationSettings settings, Guid userId)
        {
            // Validate user has admin permissions
            if (!await _permissionService.IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException("Only administrators can update federation settings");
            }

            return await _federationRepository.UpdateFederationSettingsAsync(settings);
        }

        public async Task<string> GenerateInviteUrlAsync(Guid userId, DateTime? expiresAt = null)
        {
            if (!await _permissionService.IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException("Only administrators can generate invite URLs");
            }

            var settings = await GetFederationSettingsAsync();
            if (settings == null || !settings.IsFederationEnabled)
            {
                throw new InvalidOperationException("Federation is not enabled");
            }

            var inviteId = Guid.NewGuid().ToString("N");
            var expires = expiresAt ?? DateTime.UtcNow.AddDays(7);
            
            var inviteData = new
            {
                instance_id = inviteId,
                instance_name = settings.InstanceName,
                base_url = settings.BaseUrl,
                public_key = settings.PublicKey,
                protocol_version = settings.FederationProtocolVersion,
                expires_at = expires,
                contact = settings.AdminContact
            };

            var inviteJson = JsonSerializer.Serialize(inviteData);
            var signature = await _cryptographyService.SignDataAsync(inviteJson, settings.PrivateKey);
            
            var inviteUrl = $"{settings.BaseUrl}/api/federation/invite?data={Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(inviteJson))}&signature={signature}";
            
            // Update settings with new invite URL
            settings.FederationInviteUrl = inviteUrl;
            settings.InviteUrlGeneratedAt = DateTime.UtcNow;
            settings.InviteUrlExpiresAt = expires;
            
            await _federationRepository.UpdateFederationSettingsAsync(settings);
            
            await LogFederationActivityAsync(null, userId, FederationAction.ApiKeyGenerated, "Generated federation invite URL", new { expires_at = expires });
            
            return inviteUrl;
        }

        public async Task<FederatedInstance> ConnectToInstanceAsync(string inviteUrl, Guid userId)
        {
            if (!await _permissionService.IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException("Only administrators can connect to instances");
            }

            var settings = await GetFederationSettingsAsync();
            if (settings == null || !settings.IsFederationEnabled)
            {
                throw new InvalidOperationException("Federation is not enabled");
            }

            // Parse invite URL
            var uri = new Uri(inviteUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var dataBase64 = query["data"];
            var signature = query["signature"];

            if (string.IsNullOrEmpty(dataBase64) || string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Invalid invite URL format");
            }

            var inviteJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(dataBase64));
            var inviteData = JsonSerializer.Deserialize<JsonElement>(inviteJson);
            
            var remotePublicKey = inviteData.GetProperty("public_key").GetString()!;
            var isValidSignature = await _cryptographyService.VerifySignatureAsync(inviteJson, signature, remotePublicKey);
            
            if (!isValidSignature)
            {
                throw new SecurityException("Invalid invite signature");
            }

            // Check expiration
            var expiresAt = DateTime.Parse(inviteData.GetProperty("expires_at").GetString()!);
            if (DateTime.UtcNow > expiresAt)
            {
                throw new InvalidOperationException("Invite URL has expired");
            }

            // Create federated instance
            var instance = new FederatedInstance
            {
                Id = Guid.NewGuid(),
                Name = inviteData.GetProperty("instance_name").GetString()!,
                BaseUrl = inviteData.GetProperty("base_url").GetString()!,
                PublicKey = remotePublicKey,
                Status = FederationStatus.Pending,
                AdminContact = inviteData.TryGetProperty("contact", out var contactProp) ? contactProp.GetString() : null,
                Version = inviteData.TryGetProperty("protocol_version", out var versionProp) ? versionProp.GetString() : "1.0"
            };

            await _federationRepository.AddFederatedInstanceAsync(instance);
            
            // Initiate handshake
            var handshakeResult = await InitiateHandshakeAsync(instance.Id, userId);
            
            await LogFederationActivityAsync(instance.Id, userId, FederationAction.InstanceAdded, 
                $"Connected to instance {instance.Name}", new { base_url = instance.BaseUrl });
            
            return instance;
        }

        public async Task<HandshakeResult> InitiateHandshakeAsync(Guid instanceId, Guid userId)
        {
            var instance = await _federationRepository.GetFederatedInstanceAsync(instanceId);
            if (instance == null)
            {
                throw new ArgumentException("Federated instance not found");
            }

            var settings = await GetFederationSettingsAsync();
            if (settings == null)
            {
                throw new InvalidOperationException("Federation settings not configured");
            }

            // Generate handshake challenge
            var challengeResult = await _cryptographyService.GenerateHandshakeChallengeAsync();
            
            // Create handshake record
            var handshake = new FederationHandshake
            {
                Id = Guid.NewGuid(),
                FederatedInstanceId = instanceId,
                Direction = HandshakeDirection.Outgoing,
                Status = HandshakeStatus.Initiated,
                Challenge = challengeResult.Challenge,
                ExpiresAt = challengeResult.ExpiresAt,
                InitiatedAt = DateTime.UtcNow,
                ProtocolVersion = settings.FederationProtocolVersion
            };

            await _federationRepository.AddHandshakeAsync(handshake);
            
            try
            {
                // Send handshake request to remote instance
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(settings.HandshakeTimeoutMinutes);
                
                var handshakeRequest = new
                {
                    instance_name = settings.InstanceName,
                    base_url = settings.BaseUrl,
                    public_key = settings.PublicKey,
                    challenge = challengeResult.Challenge,
                    protocol_version = settings.FederationProtocolVersion,
                    admin_contact = settings.AdminContact
                };

                var requestJson = JsonSerializer.Serialize(handshakeRequest);
                var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync($"{instance.BaseUrl}/api/federation/handshake", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);
                    
                    handshake.Response = responseData.GetProperty("signature").GetString();
                    handshake.Status = HandshakeStatus.ResponseReceived;
                    handshake.CompletedAt = DateTime.UtcNow;
                    
                    // Verify response signature
                    var responseContent = handshake.Response;
                    if (string.IsNullOrEmpty(responseContent))
                    {
                        handshake.Status = HandshakeStatus.Failed;
                        handshake.ErrorMessage = "No response received from instance";
                        await _federationRepository.UpdateHandshakeAsync(handshake);
                        
                        await LogFederationActivityAsync(instanceId, userId, FederationAction.HandshakeFailed,
                            $"Handshake failed with {instance.Name}: No response received");
                        
                        return new HandshakeResult
                        {
                            IsSuccessful = false,
                            Status = HandshakeStatus.Failed,
                            ErrorMessage = "No response received from instance"
                        };
                    }
                    
                    var isValid = await _cryptographyService.ValidateHandshakeResponseAsync(
                        challengeResult.Challenge, responseContent, instance.PublicKey);
                    
                    if (isValid)
                    {
                        handshake.Status = HandshakeStatus.Completed;
                        instance.Status = FederationStatus.Connected;
                        instance.LastHeartbeatAt = DateTime.UtcNow;
                        
                        await _federationRepository.UpdateFederatedInstanceAsync(instance);
                        
                        await LogFederationActivityAsync(instanceId, userId, FederationAction.HandshakeCompleted,
                            $"Handshake completed with {instance.Name}");
                    }
                    else
                    {
                        handshake.Status = HandshakeStatus.Failed;
                        handshake.ErrorMessage = "Invalid response signature";
                        
                        await LogFederationActivityAsync(instanceId, userId, FederationAction.HandshakeFailed,
                            $"Handshake failed with {instance.Name}: Invalid response signature");
                    }
                }
                else
                {
                    handshake.Status = HandshakeStatus.Failed;
                    handshake.ErrorMessage = $"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}";
                    
                    await LogFederationActivityAsync(instanceId, userId, FederationAction.HandshakeFailed,
                        $"Handshake failed with {instance.Name}: {handshake.ErrorMessage}");
                }
                
                await _federationRepository.UpdateHandshakeAsync(handshake);
                
                return new HandshakeResult
                {
                    IsSuccessful = handshake.Status == HandshakeStatus.Completed,
                    Status = handshake.Status,
                    ErrorMessage = handshake.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initiate handshake with instance {InstanceId}", instanceId);
                
                handshake.Status = HandshakeStatus.Failed;
                handshake.ErrorMessage = ex.Message;
                await _federationRepository.UpdateHandshakeAsync(handshake);
                
                await LogFederationActivityAsync(instanceId, userId, FederationAction.HandshakeFailed,
                    $"Handshake failed with {instance.Name}: {ex.Message}");
                
                return new HandshakeResult
                {
                    IsSuccessful = false,
                    Status = HandshakeStatus.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<IEnumerable<Model>> GetSharableModelsAsync(Guid instanceId, Guid userId)
        {
            var instance = await _federationRepository.GetFederatedInstanceAsync(instanceId);
            if (instance == null)
            {
                throw new ArgumentException("Federated instance not found");
            }

            // Check if user has permission to view models for federation
            if (!await _permissionService.HasPermissionAsync(userId, "admin.federation.manage"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions");
            }

            var settings = await GetFederationSettingsAsync();
            if (settings == null)
            {
                throw new InvalidOperationException("Federation not configured");
            }

            var query = _context.Models
                .Include(m => m.Author)
                .Include(m => m.Categories)
                .Include(m => m.Tags)
                .AsQueryable();

            // Apply federation filtering rules
            if (settings.SharePublicModelsOnly)
            {
                query = query.Where(m => m.IsPublic);
            }

            if (settings.ShareFeaturedModelsOnly)
            {
                query = query.Where(m => m.IsFeatured);
            }

            if (!settings.AllowNSFWContent)
            {
                query = query.Where(m => !m.NSFW);
            }

            // Apply instance-specific filtering
            if (instance.SyncPublicOnly)
            {
                query = query.Where(m => m.IsPublic);
            }

            if (instance.SyncFeaturedOnly)
            {
                query = query.Where(m => m.IsFeatured);
            }

            // Apply category filtering if configured
            if (!string.IsNullOrEmpty(instance.AllowedCategories))
            {
                var allowedCategoryIds = JsonSerializer.Deserialize<Guid[]>(instance.AllowedCategories);
                if (allowedCategoryIds?.Length > 0)
                {
                    query = query.Where(m => m.Categories.Any(c => allowedCategoryIds.Contains(c.Id)));
                }
            }

            // Apply role-based filtering if configured
            if (!string.IsNullOrEmpty(instance.AllowedRoles))
            {
                var allowedRoles = JsonSerializer.Deserialize<string[]>(instance.AllowedRoles);
                if (allowedRoles?.Length > 0)
                {
                    // Get users with allowed roles
                    var allowedUserIds = await _context.Users
                        .Include(u => u.Role)
                        .Where(u => u.Role != null && allowedRoles.Contains(u.Role.Name))
                        .Select(u => u.Id)
                        .ToListAsync();
                    
                    query = query.Where(m => allowedUserIds.Contains(m.AuthorId));
                }
            }

            var models = await query.Take(instance.MaxModelsToSync).ToListAsync();
            
            return models;
        }

        public async Task DisconnectFromInstanceAsync(Guid instanceId, Guid userId)
        {
            if (!await _permissionService.IsAdminAsync(userId))
            {
                throw new UnauthorizedAccessException("Only administrators can disconnect instances");
            }

            var instance = await _federationRepository.GetFederatedInstanceAsync(instanceId);
            if (instance == null)
            {
                throw new ArgumentException("Federated instance not found");
            }

            instance.Status = FederationStatus.Disabled;
            instance.IsActive = false;
            
            await _federationRepository.UpdateFederatedInstanceAsync(instance);
            
            await LogFederationActivityAsync(instanceId, userId, FederationAction.InstanceDisconnected,
                $"Disconnected from instance {instance.Name}");
        }

        private async Task LogFederationActivityAsync(Guid? instanceId, Guid? userId, FederationAction action, 
            string description, object? details = null)
        {
            var auditLog = new FederationAuditLog
            {
                Id = Guid.NewGuid(),
                FederatedInstanceId = instanceId,
                UserId = userId,
                Action = action,
                Description = description,
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                EventTimestamp = DateTime.UtcNow,
                IsSuccessful = true
            };

            await _federationRepository.AddAuditLogAsync(auditLog);
        }
    }

    public class HandshakeResult
    {
        public bool IsSuccessful { get; set; }
        public HandshakeStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
        public SecurityException(string message, Exception innerException) : base(message, innerException) { }
    }
} 