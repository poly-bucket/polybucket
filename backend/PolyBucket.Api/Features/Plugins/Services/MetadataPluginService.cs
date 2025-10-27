using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.Services
{
    public class MetadataPluginService
    {
        private readonly ILogger<MetadataPluginService> _logger;
        private readonly Dictionary<string, IMetadataPlugin> _metadataPlugins = new();
        private readonly Dictionary<string, List<MetadataField>> _entityFields = new();

        public MetadataPluginService(ILogger<MetadataPluginService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> RegisterMetadataPluginAsync(IMetadataPlugin metadataPlugin)
        {
            try
            {
                _logger.LogInformation("Registering metadata plugin {PluginId} for entity type {EntityType}", 
                    metadataPlugin.Id, metadataPlugin.EntityType);

                var key = $"{metadataPlugin.EntityType}:{metadataPlugin.Id}";
                if (_metadataPlugins.ContainsKey(key))
                {
                    _logger.LogWarning("Metadata plugin {PluginId} for entity type {EntityType} is already registered", 
                        metadataPlugin.Id, metadataPlugin.EntityType);
                    return false;
                }

                _metadataPlugins[key] = metadataPlugin;

                // Update entity fields cache
                await UpdateEntityFieldsCacheAsync(metadataPlugin.EntityType);

                _logger.LogInformation("Successfully registered metadata plugin {PluginId}", metadataPlugin.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering metadata plugin {PluginId}", metadataPlugin.Id);
                return false;
            }
        }

        public async Task<bool> UnregisterMetadataPluginAsync(string pluginId, string entityType)
        {
            try
            {
                var key = $"{entityType}:{pluginId}";
                if (_metadataPlugins.Remove(key))
                {
                    // Update entity fields cache
                    await UpdateEntityFieldsCacheAsync(entityType);
                    
                    _logger.LogInformation("Successfully unregistered metadata plugin {PluginId}", pluginId);
                    return true;
                }
                
                _logger.LogWarning("Metadata plugin {PluginId} for entity type {EntityType} was not registered", 
                    pluginId, entityType);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering metadata plugin {PluginId}", pluginId);
                return false;
            }
        }

        public async Task<List<MetadataField>> GetEntityFieldsAsync(string entityType)
        {
            try
            {
                if (_entityFields.TryGetValue(entityType, out var cachedFields))
                {
                    return cachedFields;
                }

                await UpdateEntityFieldsCacheAsync(entityType);
                return _entityFields.GetValueOrDefault(entityType, new List<MetadataField>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity fields for {EntityType}", entityType);
                return new List<MetadataField>();
            }
        }

        public async Task<MetadataValidationResult> ValidateEntityMetadataAsync(
            string entityType, 
            string entityId, 
            Dictionary<string, object> fieldValues)
        {
            try
            {
                var validationResults = new List<MetadataValidationResult>();
                var entityFields = await GetEntityFieldsAsync(entityType);

                foreach (var field in entityFields)
                {
                    if (fieldValues.TryGetValue(field.Name, out var value))
                    {
                        // Find the plugin that owns this field
                        var plugin = _metadataPlugins.Values.FirstOrDefault(p => 
                            p.EntityType == entityType && p.Fields.Any(f => f.Name == field.Name));

                        if (plugin != null)
                        {
                            var result = await plugin.ValidateFieldAsync(field.Name, value);
                            if (!result.IsValid)
                            {
                                validationResults.Add(result);
                            }
                        }
                    }
                    else if (field.Required)
                    {
                        validationResults.Add(new MetadataValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = $"Required field '{field.DisplayName}' is missing",
                            FieldName = field.Name
                        });
                    }
                }

                return new MetadataValidationResult
                {
                    IsValid = validationResults.Count == 0,
                    ErrorMessage = validationResults.Count > 0 
                        ? string.Join("; ", validationResults.Select(r => r.ErrorMessage))
                        : string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating entity metadata for {EntityType}:{EntityId}", entityType, entityId);
                return new MetadataValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Validation error: {ex.Message}"
                };
            }
        }

        public async Task<Dictionary<string, object>> TransformEntityMetadataAsync(
            string entityType,
            Dictionary<string, object> fieldValues)
        {
            try
            {
                var transformedValues = new Dictionary<string, object>();
                var entityFields = await GetEntityFieldsAsync(entityType);

                foreach (var field in entityFields)
                {
                    if (fieldValues.TryGetValue(field.Name, out var value))
                    {
                        // Find the plugin that owns this field
                        var plugin = _metadataPlugins.Values.FirstOrDefault(p => 
                            p.EntityType == entityType && p.Fields.Any(f => f.Name == field.Name));

                        if (plugin != null)
                        {
                            var transformedValue = await plugin.TransformFieldValueAsync(field.Name, value);
                            transformedValues[field.Name] = transformedValue;
                        }
                        else
                        {
                            transformedValues[field.Name] = value;
                        }
                    }
                }

                return transformedValues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming entity metadata for {EntityType}", entityType);
                return fieldValues; // Return original values if transformation fails
            }
        }

        public async Task<Dictionary<string, object>> GetDefaultEntityMetadataAsync(string entityType)
        {
            try
            {
                var defaultValues = new Dictionary<string, object>();
                var entityFields = await GetEntityFieldsAsync(entityType);

                foreach (var field in entityFields)
                {
                    if (field.DefaultValue != null)
                    {
                        defaultValues[field.Name] = field.DefaultValue;
                    }
                }

                // Get default values from plugins
                var plugins = _metadataPlugins.Values.Where(p => p.EntityType == entityType);
                foreach (var plugin in plugins)
                {
                    var pluginDefaults = await plugin.GetDefaultValuesAsync();
                    foreach (var kvp in pluginDefaults)
                    {
                        if (!defaultValues.ContainsKey(kvp.Key))
                        {
                            defaultValues[kvp.Key] = kvp.Value;
                        }
                    }
                }

                return defaultValues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default entity metadata for {EntityType}", entityType);
                return new Dictionary<string, object>();
            }
        }

        public List<MetadataPluginInfo> GetRegisteredMetadataPlugins()
        {
            var plugins = new List<MetadataPluginInfo>();
            
            foreach (var kvp in _metadataPlugins)
            {
                var plugin = kvp.Value;
                plugins.Add(new MetadataPluginInfo
                {
                    PluginId = plugin.Id,
                    PluginName = plugin.Name,
                    EntityType = plugin.EntityType,
                    FieldCount = plugin.Fields.Count,
                    IsEnabled = true // TODO: Check actual enabled status
                });
            }

            return plugins;
        }

        public bool IsMetadataPluginRegistered(string pluginId, string entityType)
        {
            var key = $"{entityType}:{pluginId}";
            return _metadataPlugins.ContainsKey(key);
        }

        private async Task UpdateEntityFieldsCacheAsync(string entityType)
        {
            try
            {
                var fields = new List<MetadataField>();
                var plugins = _metadataPlugins.Values.Where(p => p.EntityType == entityType);

                foreach (var plugin in plugins)
                {
                    fields.AddRange(plugin.Fields);
                }

                // Sort fields by order
                fields = fields.OrderBy(f => f.Order).ToList();
                _entityFields[entityType] = fields;

                _logger.LogDebug("Updated entity fields cache for {EntityType} with {FieldCount} fields", 
                    entityType, fields.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity fields cache for {EntityType}", entityType);
            }
        }
    }

    public class MetadataPluginInfo
    {
        public string PluginId { get; set; } = string.Empty;
        public string PluginName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int FieldCount { get; set; }
        public bool IsEnabled { get; set; }
    }
}
