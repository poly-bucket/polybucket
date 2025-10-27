using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.Domain
{
    public interface IMetadataPlugin : IPlugin
    {
        string EntityType { get; } // "model", "user", "collection", etc.
        List<MetadataField> Fields { get; }
        Task<MetadataValidationResult> ValidateFieldAsync(string fieldName, object value);
        Task<object> TransformFieldValueAsync(string fieldName, object value);
        Task<Dictionary<string, object>> GetDefaultValuesAsync();
    }

    public class MetadataField
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MetadataFieldType Type { get; set; }
        public bool Required { get; set; }
        public object? DefaultValue { get; set; }
        public List<string> Options { get; set; } = new(); // For select fields
        public MetadataFieldValidation? Validation { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEditable { get; set; } = true;
    }

    public enum MetadataFieldType
    {
        Text,
        Number,
        Boolean,
        Date,
        Select,
        MultiSelect,
        TextArea,
        Url,
        Email,
        Json
    }

    public class MetadataFieldValidation
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string? Pattern { get; set; } // Regex pattern
        public List<string> AllowedValues { get; set; } = new();
        public bool Required { get; set; }
    }

    public class MetadataValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    public class MetadataExtension
    {
        public string PluginId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public Dictionary<string, object> Fields { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MetadataFieldValue
    {
        public string FieldName { get; set; } = string.Empty;
        public object Value { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
