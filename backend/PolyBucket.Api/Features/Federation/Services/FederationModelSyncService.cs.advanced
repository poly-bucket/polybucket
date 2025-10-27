using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace PolyBucket.Api.Features.Federation.Services
{
    public class FederationModelSyncService(
        PolyBucketDbContext context,
        IFederationRepository federationRepository,
        IFederationCryptographyService cryptographyService,
        IHttpClientFactory httpClientFactory,
        ILogger<FederationModelSyncService> logger) : IFederationModelSyncService
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IFederationRepository _federationRepository = federationRepository;
        private readonly IFederationCryptographyService _cryptographyService = cryptographyService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<FederationModelSyncService> _logger = logger;
        private readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(5, 5);

        public async Task<SyncResult> SyncModelToInstanceAsync(Guid modelId, Guid instanceId, Guid userId)
        {
            await _syncSemaphore.WaitAsync();
            
            try
            {
                var model = await _context.Models
                    .Include(m => m.Author)
                    .Include(m => m.Categories)
                    .Include(m => m.Tags)
                    .Include(m => m.Files)
                    .FirstOrDefaultAsync(m => m.Id == modelId);

                if (model == null)
                {
                    return new SyncResult { IsSuccess = false, ErrorMessage = "Model not found" };
                }

                var instance = await _federationRepository.GetFederatedInstanceAsync(instanceId);
                if (instance == null)
                {
                    return new SyncResult { IsSuccess = false, ErrorMessage = "Federated instance not found" };
                }

                if (instance.Status != FederationStatus.Connected)
                {
                    return new SyncResult { IsSuccess = false, ErrorMessage = "Instance is not connected" };
                }

                // Check if model is already being synced
                var existingFederatedModel = await _context.FederatedModels
                    .FirstOrDefaultAsync(fm => fm.LocalModelId == modelId && fm.FederatedInstanceId == instanceId);

                if (existingFederatedModel != null && existingFederatedModel.SyncStatus == ModelSyncStatus.Syncing)
                {
                    return new SyncResult { IsSuccess = false, ErrorMessage = "Model is already being synced" };
                }

                var settings = await _federationRepository.GetFederationSettingsAsync();
                if (settings == null)
                {
                    return new SyncResult { IsSuccess = false, ErrorMessage = "Federation not configured" };
                }

                // Create or update federated model record
                var federatedModel = existingFederatedModel ?? new FederatedModel
                {
                    Id = Guid.NewGuid(),
                    LocalModelId = modelId,
                    FederatedInstanceId = instanceId,
                    Direction = FederationDirection.Outgoing
                };

                federatedModel.SyncStatus = ModelSyncStatus.Syncing;
                federatedModel.LastSyncAt = DateTime.UtcNow;
                federatedModel.SyncError = null;

                if (existingFederatedModel == null)
                {
                    await _federationRepository.AddFederatedModelAsync(federatedModel);
                }
                else
                {
                    await _federationRepository.UpdateFederatedModelAsync(federatedModel);
                }

                try
                {
                    // Prepare model data for transmission
                    var modelData = PrepareModelDataForSync(model, instance, settings);
                    
                    // Send model data to remote instance
                    var result = await SendModelDataToInstanceAsync(modelData, instance, settings);
                    
                    if (result.IsSuccess)
                    {
                        federatedModel.SyncStatus = ModelSyncStatus.Synced;
                        federatedModel.MetadataHash = await _cryptographyService.HashDataAsync(JsonSerializer.Serialize(modelData));
                        
                        // Update instance statistics
                        instance.ModelsShared++;
                        instance.LastSyncAt = DateTime.UtcNow;
                        await _federationRepository.UpdateFederatedInstanceAsync(instance);
                        
                        await LogSyncActivityAsync(federatedModel, FederationAction.ModelShared, "Model successfully synced", userId);
                    }
                    else
                    {
                        federatedModel.SyncStatus = ModelSyncStatus.Failed;
                        federatedModel.SyncError = result.ErrorMessage;
                        
                        await LogSyncActivityAsync(federatedModel, FederationAction.ModelSyncFailed, 
                            $"Model sync failed: {result.ErrorMessage}", userId);
                    }

                    await _federationRepository.UpdateFederatedModelAsync(federatedModel);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing model {ModelId} to instance {InstanceId}", modelId, instanceId);
                    
                    federatedModel.SyncStatus = ModelSyncStatus.Failed;
                    federatedModel.SyncError = ex.Message;
                    await _federationRepository.UpdateFederatedModelAsync(federatedModel);
                    
                    await LogSyncActivityAsync(federatedModel, FederationAction.ModelSyncFailed, 
                        $"Model sync failed with exception: {ex.Message}", userId);
                    
                    return new SyncResult { IsSuccess = false, ErrorMessage = ex.Message };
                }
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        public async Task<SyncResult> SyncInstanceModelsAsync(Guid instanceId, Guid userId)
        {
            var instance = await _federationRepository.GetFederatedInstanceAsync(instanceId);
            if (instance == null)
            {
                return new SyncResult { IsSuccess = false, ErrorMessage = "Federated instance not found" };
            }

            if (instance.Status != FederationStatus.Connected)
            {
                return new SyncResult { IsSuccess = false, ErrorMessage = "Instance is not connected" };
            }

            var settings = await _federationRepository.GetFederationSettingsAsync();
            if (settings == null)
            {
                return new SyncResult { IsSuccess = false, ErrorMessage = "Federation not configured" };
            }

            try
            {
                // Get available models from remote instance
                var remoteModels = await FetchRemoteModelsAsync(instance, settings);
                
                var syncCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                foreach (var remoteModel in remoteModels)
                {
                    try
                    {
                        var result = await ProcessIncomingModelAsync(remoteModel, instance, settings, userId);
                        
                        if (result.IsSuccess)
                        {
                            syncCount++;
                        }
                        else
                        {
                            errorCount++;
                            errors.Add($"Model {remoteModel.Id}: {result.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"Model {remoteModel.Id}: {ex.Message}");
                        _logger.LogError(ex, "Error processing incoming model {ModelId} from instance {InstanceId}", 
                            remoteModel.Id, instanceId);
                    }
                }

                // Update instance statistics
                instance.ModelsReceived += syncCount;
                instance.LastSyncAt = DateTime.UtcNow;
                await _federationRepository.UpdateFederatedInstanceAsync(instance);

                var message = $"Synced {syncCount} models";
                if (errorCount > 0)
                {
                    message += $", {errorCount} errors";
                }

                return new SyncResult 
                { 
                    IsSuccess = errorCount == 0, 
                    ErrorMessage = errorCount > 0 ? string.Join("; ", errors) : null,
                    Details = new { SyncedCount = syncCount, ErrorCount = errorCount, Errors = errors }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing models from instance {InstanceId}", instanceId);
                return new SyncResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        private ModelSyncData PrepareModelDataForSync(Model model, FederatedInstance instance, FederationSettings settings)
        {
            var modelData = new ModelSyncData
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                AuthorName = model.Author.Username,
                AuthorId = model.AuthorId,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt ?? model.CreatedAt,
                ThumbnailUrl = model.ThumbnailUrl,
                Downloads = model.Downloads,
                Likes = model.Likes,
                Categories = model.Categories.Select(c => c.Name).ToArray(),
                Tags = model.Tags.Select(t => t.Name).ToArray(),
                License = model.License?.ToString(),
                IsNSFW = model.NSFW,
                IsAIGenerated = model.AIGenerated,
                FileSize = model.Files.Sum(f => f.Size),
                FileFormats = model.Files.Select(f => System.IO.Path.GetExtension(f.Name)).Distinct().ToArray()
            };

            return modelData;
        }

        private async Task<SyncResult> SendModelDataToInstanceAsync(ModelSyncData modelData, FederatedInstance instance, FederationSettings settings)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(10);

                var secureMessage = await _cryptographyService.CreateSecureMessageAsync(modelData, settings.PrivateKey);
                var content = new StringContent(secureMessage, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{instance.BaseUrl}/api/federation/models/receive", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    return new SyncResult
                    {
                        IsSuccess = true,
                        Details = responseData
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new SyncResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"HTTP {response.StatusCode}: {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send model data to instance {InstanceId}", instance.Id);
                return new SyncResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<IEnumerable<RemoteModelInfo>> FetchRemoteModelsAsync(FederatedInstance instance, FederationSettings settings)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5);

                var requestData = new
                {
                    instance_id = settings.InstanceName,
                    last_sync = instance.LastSyncAt?.ToString("O")
                };

                var secureMessage = await _cryptographyService.CreateSecureMessageAsync(requestData, settings.PrivateKey);
                var content = new StringContent(secureMessage, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{instance.BaseUrl}/api/federation/models/list", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var models = await _cryptographyService.VerifyAndDecodeSecureMessageAsync<RemoteModelInfo[]>(responseContent, instance.PublicKey);
                    
                    return models ?? Array.Empty<RemoteModelInfo>();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch models from instance {InstanceId}: HTTP {StatusCode}", 
                        instance.Id, response.StatusCode);
                    return Array.Empty<RemoteModelInfo>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching models from instance {InstanceId}", instance.Id);
                return Array.Empty<RemoteModelInfo>();
            }
        }

        private async Task<SyncResult> ProcessIncomingModelAsync(RemoteModelInfo remoteModel, FederatedInstance instance, FederationSettings settings, Guid userId)
        {
            // Check if we already have this model
            var existingModel = await _context.FederatedModels
                .FirstOrDefaultAsync(fm => fm.RemoteModelId == remoteModel.Id && fm.FederatedInstanceId == instance.Id);

            if (existingModel != null)
            {
                // Check if model has been updated
                var remoteHash = await _cryptographyService.HashDataAsync(JsonSerializer.Serialize(remoteModel));
                if (existingModel.MetadataHash == remoteHash)
                {
                    return new SyncResult { IsSuccess = true, ErrorMessage = "Model already up to date" };
                }
            }

            // Create or update federated model record
            var federatedModel = existingModel ?? new FederatedModel
            {
                Id = Guid.NewGuid(),
                FederatedInstanceId = instance.Id,
                RemoteModelId = remoteModel.Id,
                Direction = FederationDirection.Incoming
            };

            // Update model metadata
            federatedModel.RemoteModelName = remoteModel.Name;
            federatedModel.RemoteModelDescription = remoteModel.Description;
            federatedModel.RemoteAuthorName = remoteModel.AuthorName;
            federatedModel.RemoteCreatedAt = remoteModel.CreatedAt;
            federatedModel.RemoteUpdatedAt = remoteModel.UpdatedAt;
            federatedModel.RemoteThumbnailUrl = remoteModel.ThumbnailUrl;
            federatedModel.RemoteDownloads = remoteModel.Downloads;
            federatedModel.RemoteLikes = remoteModel.Likes;
            federatedModel.RemoteCategories = JsonSerializer.Serialize(remoteModel.Categories);
            federatedModel.RemoteTags = JsonSerializer.Serialize(remoteModel.Tags);
            federatedModel.RemoteLicense = remoteModel.License;
            federatedModel.RemoteIsNSFW = remoteModel.IsNSFW;
            federatedModel.RemoteIsAIGenerated = remoteModel.IsAIGenerated;
            federatedModel.RemoteFileSize = remoteModel.FileSize;
            federatedModel.RemoteFileFormats = JsonSerializer.Serialize(remoteModel.FileFormats);
            federatedModel.SyncStatus = ModelSyncStatus.Synced;
            federatedModel.LastSyncAt = DateTime.UtcNow;
            federatedModel.MetadataHash = await _cryptographyService.HashDataAsync(JsonSerializer.Serialize(remoteModel));

            if (existingModel == null)
            {
                await _federationRepository.AddFederatedModelAsync(federatedModel);
            }
            else
            {
                await _federationRepository.UpdateFederatedModelAsync(federatedModel);
            }

            await LogSyncActivityAsync(federatedModel, FederationAction.ModelReceived, 
                $"Received model metadata from {instance.Name}", userId);

            return new SyncResult { IsSuccess = true };
        }

        private async Task LogSyncActivityAsync(FederatedModel federatedModel, FederationAction action, string description, Guid? userId)
        {
            var auditLog = new FederationAuditLog
            {
                Id = Guid.NewGuid(),
                FederatedInstanceId = federatedModel.FederatedInstanceId,
                UserId = userId,
                Action = action,
                Description = description,
                AffectedResourceType = "Model",
                AffectedResourceId = federatedModel.LocalModelId?.ToString() ?? federatedModel.RemoteModelId?.ToString(),
                EventTimestamp = DateTime.UtcNow,
                IsSuccessful = action != FederationAction.ModelSyncFailed
            };

            await _federationRepository.AddAuditLogAsync(auditLog);
        }
    }

    public class ModelSyncData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int Downloads { get; set; }
        public int Likes { get; set; }
        public string[] Categories { get; set; } = Array.Empty<string>();
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string? License { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsAIGenerated { get; set; }
        public long FileSize { get; set; }
        public string[] FileFormats { get; set; } = Array.Empty<string>();
    }

    public class RemoteModelInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int Downloads { get; set; }
        public int Likes { get; set; }
        public string[] Categories { get; set; } = Array.Empty<string>();
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string? License { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsAIGenerated { get; set; }
        public long FileSize { get; set; }
        public string[] FileFormats { get; set; } = Array.Empty<string>();
    }

    public class SyncResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public object? Details { get; set; }
    }
} 