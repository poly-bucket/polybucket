using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace PolyBucket.Api.Features.Federation.Plugins
{
    public class DefaultFederationPlugin : IFederationPlugin
    {
        public string Id => "default-federation-plugin";
        public string Name => "Default Federation Plugin";
        public string Version => "1.0.0";
        public string Author => "PolyBucket Team";
        public string Description => "Default federation plugin with basic model sharing rules and security validations";

        public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>
        {
            new PluginComponent
            {
                Id = "federation-status-widget",
                Name = "Federation Status Widget",
                ComponentPath = "plugins/federation/FederationStatusWidget",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "admin-dashboard-sidebar",
                        ComponentId = "federation-status-widget",
                        Priority = 20,
                        Config = new Dictionary<string, object>
                        {
                            { "showConnectedInstances", true },
                            { "showSyncStats", true }
                        }
                    }
                }
            },
            new PluginComponent
            {
                Id = "model-federation-controls",
                Name = "Model Federation Controls",
                ComponentPath = "plugins/federation/ModelFederationControls",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "model-details-actions",
                        ComponentId = "model-federation-controls",
                        Priority = 30,
                        Config = new Dictionary<string, object>
                        {
                            { "showSyncButton", true },
                            { "showFederationStatus", true }
                        }
                    }
                }
            }
        };

        public PluginMetadata Metadata => new PluginMetadata
        {
            MinimumAppVersion = "1.0.0",
            RequiredPermissions = new List<string>
            {
                "federation.view",
                "federation.sync"
            },
            OptionalPermissions = new List<string>
            {
                "admin.federation.manage"
            },
            Settings = new Dictionary<string, PluginSetting>
            {
                ["auto_sync_enabled"] = new PluginSetting
                {
                    Name = "Auto-sync Enabled",
                    Description = "Automatically sync new public models to connected instances",
                    Type = PluginSettingType.Boolean,
                    DefaultValue = false,
                    Required = false
                },
                ["sync_interval_hours"] = new PluginSetting
                {
                    Name = "Sync Interval (Hours)",
                    Description = "How often to check for new models to sync",
                    Type = PluginSettingType.Number,
                    DefaultValue = 24,
                    Required = false
                },
                ["max_file_size_mb"] = new PluginSetting
                {
                    Name = "Max File Size (MB)",
                    Description = "Maximum file size to sync via federation",
                    Type = PluginSettingType.Number,
                    DefaultValue = 100,
                    Required = false
                },
                ["blocked_categories"] = new PluginSetting
                {
                    Name = "Blocked Categories",
                    Description = "Categories that should never be synced",
                    Type = PluginSettingType.MultiSelect,
                    DefaultValue = new string[0],
                    Required = false,
                    Options = new List<string> { "Adult", "Weapons", "Illegal" }
                }
            },
            Lifecycle = new PluginLifecycle
            {
                AutoStart = true,
                CanDisable = false,
                CanUninstall = false
            }
        };

        public async Task InitializeAsync()
        {
            // Initialize default federation rules and settings
            await Task.CompletedTask;
        }

        public async Task UnloadAsync()
        {
            // Cleanup plugin resources
            await Task.CompletedTask;
        }

        public async Task<bool> ShouldShareModelAsync(Model model, FederatedInstance instance, Guid userId)
        {
            // Basic validation rules for sharing models
            
            // Don't share private models unless specifically configured
            if (!model.IsPublic && instance.SyncPublicOnly)
            {
                return false;
            }

            // Don't share NSFW content unless allowed
            if (model.NSFW && !instance.AllowedCategories.Contains("NSFW"))
            {
                return false;
            }

            // Check file size limits
            var totalFileSize = model.Files?.Sum(f => f.Size) ?? 0;
            var maxFileSize = GetPluginSetting<long>("max_file_size_mb", 100) * 1024 * 1024;
            if (totalFileSize > maxFileSize)
            {
                return false;
            }

            // Check blocked categories
            var blockedCategories = GetPluginSetting<string[]>("blocked_categories", new string[0]);
            if (model.Categories?.Any(c => blockedCategories.Contains(c.Name)) == true)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ShouldAcceptModelAsync(FederatedModel federatedModel, FederatedInstance instance)
        {
            // Validate incoming models from federated instances
            
            // Don't accept NSFW content if not allowed
            if (federatedModel.RemoteIsNSFW)
            {
                var allowNSFW = GetPluginSetting<bool>("allow_nsfw", false);
                if (!allowNSFW)
                {
                    return false;
                }
            }

            // Check file size limits
            var maxFileSize = GetPluginSetting<long>("max_file_size_mb", 100) * 1024 * 1024;
            if (federatedModel.RemoteFileSize > maxFileSize)
            {
                return false;
            }

            // Check instance trust level
            if (!instance.IsTrusted && federatedModel.RemoteIsAIGenerated)
            {
                // Be more cautious with AI-generated content from untrusted instances
                return false;
            }

            return true;
        }

        public async Task<Dictionary<string, object>> TransformModelMetadataAsync(Model model, FederatedInstance targetInstance)
        {
            var metadata = new Dictionary<string, object>
            {
                ["id"] = model.Id.ToString(),
                ["name"] = model.Name,
                ["description"] = model.Description,
                ["author"] = model.Author?.Username ?? "Unknown",
                ["created_at"] = model.CreatedAt.ToString("O"),
                ["updated_at"] = (model.UpdatedAt ?? model.CreatedAt).ToString("O"),
                ["categories"] = model.Categories?.Select(c => c.Name).ToArray() ?? Array.Empty<string>(),
                ["tags"] = model.Tags?.Select(t => t.Name).ToArray() ?? Array.Empty<string>(),
                ["license"] = model.License?.ToString() ?? "Unknown",
                ["is_nsfw"] = model.NSFW,
                ["is_ai_generated"] = model.AIGenerated,
                ["downloads"] = model.Downloads,
                ["likes"] = model.Likes,
                ["file_count"] = model.Files?.Count ?? 0,
                ["total_file_size"] = model.Files?.Sum(f => f.Size) ?? 0
            };

            // Add plugin-specific metadata
            metadata["federation_plugin"] = Id;
            metadata["federation_timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // Add instance-specific transformations
            if (targetInstance.IsTrusted)
            {
                metadata["trust_level"] = "high";
                metadata["extended_metadata"] = true;
            }
            else
            {
                metadata["trust_level"] = "standard";
                metadata["extended_metadata"] = false;
            }

            return metadata;
        }

        public async Task<Dictionary<string, object>> ProcessIncomingModelMetadataAsync(Dictionary<string, object> metadata, FederatedInstance sourceInstance)
        {
            // Process and validate incoming metadata
            var processedMetadata = new Dictionary<string, object>(metadata);

            // Add source instance information
            processedMetadata["source_instance"] = sourceInstance.Name;
            processedMetadata["source_instance_id"] = sourceInstance.Id.ToString();
            processedMetadata["received_at"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Validate required fields
            var requiredFields = new[] { "id", "name", "author", "created_at" };
            foreach (var field in requiredFields)
            {
                if (!processedMetadata.ContainsKey(field))
                {
                    throw new ArgumentException($"Required metadata field '{field}' is missing");
                }
            }

            // Sanitize description and name
            if (processedMetadata.TryGetValue("description", out var desc))
            {
                processedMetadata["description"] = SanitizeText(desc.ToString() ?? "");
            }

            if (processedMetadata.TryGetValue("name", out var name))
            {
                processedMetadata["name"] = SanitizeText(name.ToString() ?? "");
            }

            return processedMetadata;
        }

        public async Task<HandshakeValidationResult> ValidateHandshakeAsync(FederationHandshake handshake, FederatedInstance instance)
        {
            var result = new HandshakeValidationResult { IsValid = true };

            try
            {
                // Parse handshake challenge
                var challenge = JsonSerializer.Deserialize<JsonElement>(handshake.Challenge ?? "{}");
                
                // Validate timestamp (must be within last 10 minutes)
                if (challenge.TryGetProperty("timestamp", out var timestampProp))
                {
                    var timestamp = DateTimeOffset.FromUnixTimeSeconds(timestampProp.GetInt64());
                    var age = DateTimeOffset.UtcNow - timestamp;
                    
                    if (age.TotalMinutes > 10)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = "Handshake challenge is too old";
                        return result;
                    }
                }

                // Validate protocol version compatibility
                if (handshake.ProtocolVersion != "1.0")
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Unsupported protocol version: {handshake.ProtocolVersion}";
                    return result;
                }

                // Additional instance-specific validations
                if (handshake.Direction == HandshakeDirection.Incoming)
                {
                    result.AdditionalData["requires_manual_approval"] = !instance.IsTrusted;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Handshake validation error: {ex.Message}";
                return result;
            }
        }

        public async Task<string?> GenerateSecurityChallengeAsync(FederatedInstance instance)
        {
            // Generate additional security challenge based on instance trust level
            if (instance.IsTrusted)
            {
                return null; // No additional challenge needed for trusted instances
            }

            var challenge = new
            {
                challenge_type = "basic_auth",
                nonce = Guid.NewGuid().ToString("N"),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                instance_id = instance.Id.ToString()
            };

            return JsonSerializer.Serialize(challenge);
        }

        public async Task<bool> ValidateSecurityResponseAsync(string challenge, string response, FederatedInstance instance)
        {
            try
            {
                var challengeData = JsonSerializer.Deserialize<JsonElement>(challenge);
                var responseData = JsonSerializer.Deserialize<JsonElement>(response);

                // Validate that the response contains the expected nonce
                if (challengeData.TryGetProperty("nonce", out var expectedNonce) &&
                    responseData.TryGetProperty("nonce", out var actualNonce))
                {
                    return expectedNonce.GetString() == actualNonce.GetString();
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Model>> FilterDiscoverableModelsAsync(IEnumerable<Model> models, FederatedInstance requestingInstance)
        {
            return models.Where(model =>
            {
                // Only show public models to other instances
                if (!model.IsPublic) return false;

                // Filter out NSFW content unless specifically allowed
                if (model.NSFW && !requestingInstance.AllowedCategories.Contains("NSFW")) return false;

                // Filter by file size
                var totalSize = model.Files?.Sum(f => f.Size) ?? 0;
                var maxSize = GetPluginSetting<long>("max_file_size_mb", 100) * 1024 * 1024;
                if (totalSize > maxSize) return false;

                return true;
            });
        }

        public async Task<Dictionary<string, object>> EnrichModelDataAsync(Model model, FederatedInstance targetInstance)
        {
            var enrichment = new Dictionary<string, object>();

            // Add compatibility information
            enrichment["compatibility"] = new
            {
                supports_3d_preview = model.Files?.Any(f => f.Name.EndsWith(".stl") || f.Name.EndsWith(".obj")) ?? false,
                supports_direct_download = true,
                estimated_print_time = CalculateEstimatedPrintTime(model)
            };

            // Add quality metrics
            enrichment["quality_metrics"] = new
            {
                download_popularity = model.Downloads > 100 ? "high" : model.Downloads > 10 ? "medium" : "low",
                community_rating = model.Likes > 50 ? "excellent" : model.Likes > 10 ? "good" : "new",
                author_reputation = "verified" // Could be calculated based on author's other models
            };

            return enrichment;
        }

        public async Task OnFederationEventAsync(FederationEvent federationEvent)
        {
            // Handle federation events for logging, notifications, etc.
            switch (federationEvent.EventType)
            {
                case "model_shared":
                    // Log successful model sharing
                    break;
                case "handshake_completed":
                    // Notify about new instance connection
                    break;
                case "sync_failed":
                    // Handle sync failures
                    break;
            }
        }

        public async Task<SettingsValidationResult> ValidateFederationSettingsAsync(FederationSettings oldSettings, FederationSettings newSettings, Guid userId)
        {
            var result = new SettingsValidationResult { IsValid = true };

            // Validate instance name
            if (string.IsNullOrWhiteSpace(newSettings.InstanceName) || newSettings.InstanceName.Length < 3)
            {
                result.Errors.Add("Instance name must be at least 3 characters long");
                result.IsValid = false;
            }

            // Validate base URL
            if (!Uri.TryCreate(newSettings.BaseUrl, UriKind.Absolute, out var uri))
            {
                result.Errors.Add("Base URL must be a valid absolute URL");
                result.IsValid = false;
            }
            else if (uri.Scheme != "https" && newSettings.RequireHttps)
            {
                result.Errors.Add("HTTPS is required but base URL uses HTTP");
                result.IsValid = false;
            }

            // Validate admin contact
            if (string.IsNullOrWhiteSpace(newSettings.AdminContact))
            {
                result.Warnings.Add("Admin contact is not specified");
            }

            return result;
        }

        public async Task<SyncStrategy?> GetSyncStrategyAsync(Model model, FederatedInstance targetInstance)
        {
            // Determine sync strategy based on model and instance characteristics
            
            if (model.IsFeatured)
            {
                return new SyncStrategy
                {
                    Name = "priority_sync",
                    Priority = 1,
                    SyncInterval = TimeSpan.FromMinutes(30),
                    RequiresFullSync = true
                };
            }

            if (model.Downloads > 1000)
            {
                return new SyncStrategy
                {
                    Name = "popular_model_sync",
                    Priority = 10,
                    SyncInterval = TimeSpan.FromHours(2),
                    RequiresFullSync = false
                };
            }

            return new SyncStrategy
            {
                Name = "standard_sync",
                Priority = 100,
                SyncInterval = TimeSpan.FromHours(24),
                RequiresFullSync = false
            };
        }

        public async Task OnPreSyncAsync(FederatedModel federatedModel)
        {
            // Pre-sync validation and preparation
            await Task.CompletedTask;
        }

        public async Task OnPostSyncAsync(FederatedModel federatedModel, bool successful)
        {
            // Post-sync cleanup and notifications
            if (successful)
            {
                // Update sync statistics
            }
            else
            {
                // Handle sync failure
            }
        }

        public async Task<Dictionary<string, object>?> EnrichAuditLogAsync(FederationAuditLog auditLog)
        {
            var enrichment = new Dictionary<string, object>
            {
                ["plugin_version"] = Version,
                ["enriched_at"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Add context-specific enrichment
            if (auditLog.Action == FederationAction.ModelShared)
            {
                enrichment["sharing_method"] = "automatic";
                enrichment["data_sensitivity"] = "public";
            }

            return enrichment;
        }

        #region Helper Methods

        private T GetPluginSetting<T>(string key, T defaultValue)
        {
            // In a real implementation, this would read from the plugin settings storage
            return defaultValue;
        }

        private string SanitizeText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            // Basic sanitization - remove/escape potentially harmful content
            return input.Replace("<script>", "&lt;script&gt;")
                       .Replace("</script>", "&lt;/script&gt;")
                       .Trim();
        }

        private TimeSpan CalculateEstimatedPrintTime(Model model)
        {
            // Simple estimation based on file size
            var totalSize = model.Files?.Sum(f => f.Size) ?? 0;
            var estimatedMinutes = Math.Max(30, totalSize / (1024 * 1024) * 15); // ~15 min per MB
            return TimeSpan.FromMinutes(estimatedMinutes);
        }

        #endregion
    }
} 