using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.Services
{
    public class PluginManifestValidator
    {
        private readonly ILogger<PluginManifestValidator> _logger;

        public PluginManifestValidator(ILogger<PluginManifestValidator> logger)
        {
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateManifestAsync(string manifestPath)
        {
            try
            {
                if (!File.Exists(manifestPath))
                {
                    return ValidationResult.Failed("Plugin manifest file not found");
                }

                var manifestContent = await File.ReadAllTextAsync(manifestPath);
                var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (manifest == null)
                {
                    return ValidationResult.Failed("Failed to parse plugin manifest");
                }

                return await ValidateManifestAsync(manifest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating plugin manifest at {ManifestPath}", manifestPath);
                return ValidationResult.Failed($"Error validating manifest: {ex.Message}");
            }
        }

        public Task<ValidationResult> ValidateManifestAsync(PluginManifest manifest)
        {
            var errors = new List<string>();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(manifest.Id))
                errors.Add("Plugin ID is required");

            if (string.IsNullOrWhiteSpace(manifest.Name))
                errors.Add("Plugin name is required");

            if (string.IsNullOrWhiteSpace(manifest.Version))
                errors.Add("Plugin version is required");

            if (string.IsNullOrWhiteSpace(manifest.Author))
                errors.Add("Plugin author is required");

            if (string.IsNullOrWhiteSpace(manifest.Description))
                errors.Add("Plugin description is required");

            if (string.IsNullOrWhiteSpace(manifest.Type))
                errors.Add("Plugin type is required");

            // Validate version format
            if (!string.IsNullOrWhiteSpace(manifest.Version) && !Version.TryParse(manifest.Version, out _))
                errors.Add("Invalid version format");

            // Validate compatibility
            if (manifest.Compatibility != null)
            {
                if (!string.IsNullOrWhiteSpace(manifest.Compatibility.MinVersion) && 
                    !Version.TryParse(manifest.Compatibility.MinVersion, out _))
                    errors.Add("Invalid minimum version format");

                if (!string.IsNullOrWhiteSpace(manifest.Compatibility.MaxVersion) && 
                    !Version.TryParse(manifest.Compatibility.MaxVersion, out _))
                    errors.Add("Invalid maximum version format");
            }

            // Validate plugin type
            var validTypes = new[] { "theme", "oauth", "metadata", "localization", "layout", "integration" };
            if (!string.IsNullOrWhiteSpace(manifest.Type) && !validTypes.Contains(manifest.Type.ToLower()))
                errors.Add($"Invalid plugin type. Must be one of: {string.Join(", ", validTypes)}");

            // Validate settings
            if (manifest.Settings != null)
            {
                foreach (var setting in manifest.Settings)
                {
                    if (string.IsNullOrWhiteSpace(setting.Key))
                        errors.Add("Plugin setting key is required");

                    if (string.IsNullOrWhiteSpace(setting.Type))
                        errors.Add($"Plugin setting type is required for key '{setting.Key}'");

                    var validSettingTypes = new[] { "string", "number", "boolean", "color", "select" };
                    if (!string.IsNullOrWhiteSpace(setting.Type) && !validSettingTypes.Contains(setting.Type.ToLower()))
                        errors.Add($"Invalid setting type '{setting.Type}' for key '{setting.Key}'. Must be one of: {string.Join(", ", validSettingTypes)}");
                }
            }

            if (errors.Count > 0)
            {
                return Task.FromResult(ValidationResult.Failed($"Plugin manifest validation failed: {string.Join("; ", errors)}"));
            }

            return Task.FromResult(ValidationResult.Success());
        }

        public bool IsCompatible(PluginCompatibility compatibility, string currentVersion = "1.0.0")
        {
            try
            {
                var current = new Version(currentVersion);
                var minVersion = string.IsNullOrWhiteSpace(compatibility.MinVersion) ? new Version("1.0.0") : new Version(compatibility.MinVersion);
                var maxVersion = string.IsNullOrWhiteSpace(compatibility.MaxVersion) ? new Version("999.0.0") : new Version(compatibility.MaxVersion);

                return current >= minVersion && current <= maxVersion;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failed(string message)
        {
            return new ValidationResult { IsValid = false, Message = message };
        }
    }
}
