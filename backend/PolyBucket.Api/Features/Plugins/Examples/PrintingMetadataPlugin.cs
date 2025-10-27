using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.Domain;
using PluginComponent = PolyBucket.Api.Common.Plugins.PluginComponent;
using PluginHook = PolyBucket.Api.Common.Plugins.PluginHook;
using PluginSetting = PolyBucket.Api.Common.Plugins.PluginSetting;

namespace PolyBucket.Api.Features.Plugins.Examples
{
    public class PrintingMetadataPlugin : IPlugin, IMetadataPlugin
    {
        private readonly ILogger<PrintingMetadataPlugin> _logger;

        public PrintingMetadataPlugin(ILogger<PrintingMetadataPlugin> logger)
        {
            _logger = logger;
        }

        public string Id => "printing-metadata-plugin";
        public string Name => "3D Printing Metadata";
        public string Version => "1.0.0";
        public string Author => "PolyBucket Community";
        public string Description => "Adds 3D printing specific metadata fields to models";

        public string EntityType => "model";

        public List<MetadataField> Fields => new()
        {
            new MetadataField
            {
                Name = "printTime",
                DisplayName = "Print Time",
                Description = "Estimated print time in hours",
                Type = MetadataFieldType.Number,
                Required = false,
                DefaultValue = 0.0,
                Validation = new MetadataFieldValidation
                {
                    MinValue = 0,
                    MaxValue = 1000
                },
                Order = 1,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "filamentType",
                DisplayName = "Filament Type",
                Description = "Type of filament used",
                Type = MetadataFieldType.Select,
                Required = false,
                DefaultValue = "PLA",
                Options = new List<string> { "PLA", "ABS", "PETG", "TPU", "Wood", "Metal", "Carbon Fiber" },
                Order = 2,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "layerHeight",
                DisplayName = "Layer Height",
                Description = "Layer height in millimeters",
                Type = MetadataFieldType.Number,
                Required = false,
                DefaultValue = 0.2,
                Validation = new MetadataFieldValidation
                {
                    MinValue = 0.05,
                    MaxValue = 1.0
                },
                Order = 3,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "infillPercentage",
                DisplayName = "Infill Percentage",
                Description = "Infill percentage (0-100)",
                Type = MetadataFieldType.Number,
                Required = false,
                DefaultValue = 20,
                Validation = new MetadataFieldValidation
                {
                    MinValue = 0,
                    MaxValue = 100
                },
                Order = 4,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "supports",
                DisplayName = "Supports Required",
                Description = "Whether supports are required for printing",
                Type = MetadataFieldType.Boolean,
                Required = false,
                DefaultValue = false,
                Order = 5,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "bedTemperature",
                DisplayName = "Bed Temperature",
                Description = "Recommended bed temperature in Celsius",
                Type = MetadataFieldType.Number,
                Required = false,
                DefaultValue = 60,
                Validation = new MetadataFieldValidation
                {
                    MinValue = 0,
                    MaxValue = 200
                },
                Order = 6,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "nozzleTemperature",
                DisplayName = "Nozzle Temperature",
                Description = "Recommended nozzle temperature in Celsius",
                Type = MetadataFieldType.Number,
                Required = false,
                DefaultValue = 200,
                Validation = new MetadataFieldValidation
                {
                    MinValue = 0,
                    MaxValue = 300
                },
                Order = 7,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "printSpeed",
                DisplayName = "Print Speed",
                Description = "Recommended print speed in mm/s",
                Type = MetadataFieldType.Number,
                Required = false,
                DefaultValue = 50,
                Validation = new MetadataFieldValidation
                {
                    MinValue = 1,
                    MaxValue = 200
                },
                Order = 8,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "difficulty",
                DisplayName = "Print Difficulty",
                Description = "Difficulty level for printing this model",
                Type = MetadataFieldType.Select,
                Required = false,
                DefaultValue = "Beginner",
                Options = new List<string> { "Beginner", "Intermediate", "Advanced", "Expert" },
                Order = 9,
                IsVisible = true,
                IsEditable = true
            },
            new MetadataField
            {
                Name = "notes",
                DisplayName = "Printing Notes",
                Description = "Additional notes for printing this model",
                Type = MetadataFieldType.TextArea,
                Required = false,
                DefaultValue = "",
                Validation = new MetadataFieldValidation
                {
                    MaxLength = 1000
                },
                Order = 10,
                IsVisible = true,
                IsEditable = true
            }
        };

        public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>
        {
            new PluginComponent
            {
                Id = "printing-metadata-form",
                Name = "Printing Metadata Form",
                ComponentPath = "plugins/printing-metadata/PrintingMetadataForm",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "model-edit-form",
                        ComponentId = "printing-metadata-form",
                        Priority = 100
                    }
                }
            },
            new PluginComponent
            {
                Id = "printing-metadata-display",
                Name = "Printing Metadata Display",
                ComponentPath = "plugins/printing-metadata/PrintingMetadataDisplay",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "model-details",
                        ComponentId = "printing-metadata-display",
                        Priority = 100
                    }
                }
            }
        };

        public PluginMetadata Metadata => new PluginMetadata
        {
            MinimumAppVersion = "1.0.0",
            RequiredPermissions = new List<string> { "metadata.extend", "model.edit" },
            OptionalPermissions = new List<string> { "admin.metadata" },
            Settings = new Dictionary<string, PluginSetting>
            {
                ["enablePrintTimeEstimation"] = new PluginSetting
                {
                    Name = "Enable Print Time Estimation",
                    Description = "Automatically estimate print time based on model complexity",
                    Type = PluginSettingType.Boolean,
                    DefaultValue = true,
                    Required = false
                },
                ["defaultFilamentType"] = new PluginSetting
                {
                    Name = "Default Filament Type",
                    Description = "Default filament type for new models",
                    Type = PluginSettingType.String,
                    DefaultValue = "PLA",
                    Required = false
                },
                ["requirePrintingMetadata"] = new PluginSetting
                {
                    Name = "Require Printing Metadata",
                    Description = "Require printing metadata for all models",
                    Type = PluginSettingType.Boolean,
                    DefaultValue = false,
                    Required = false
                }
            },
            Lifecycle = new PluginLifecycle
            {
                AutoStart = true,
                CanDisable = true,
                CanUninstall = true
            }
        };

        public async Task<MetadataValidationResult> ValidateFieldAsync(string fieldName, object value)
        {
            try
            {
                var field = Fields.FirstOrDefault(f => f.Name == fieldName);
                if (field == null)
                {
                    return new MetadataValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Unknown field: {fieldName}",
                        FieldName = fieldName,
                        Value = value
                    };
                }

                // Type-specific validation
                switch (field.Type)
                {
                    case MetadataFieldType.Number:
                        if (value is double doubleValue)
                        {
                            if (field.Validation?.MinValue.HasValue == true && doubleValue < field.Validation.MinValue.Value)
                            {
                                return new MetadataValidationResult
                                {
                                    IsValid = false,
                                    ErrorMessage = $"{field.DisplayName} must be at least {field.Validation.MinValue.Value}",
                                    FieldName = fieldName,
                                    Value = value
                                };
                            }
                            if (field.Validation?.MaxValue.HasValue == true && doubleValue > field.Validation.MaxValue.Value)
                            {
                                return new MetadataValidationResult
                                {
                                    IsValid = false,
                                    ErrorMessage = $"{field.DisplayName} must be at most {field.Validation.MaxValue.Value}",
                                    FieldName = fieldName,
                                    Value = value
                                };
                            }
                        }
                        else
                        {
                            return new MetadataValidationResult
                            {
                                IsValid = false,
                                ErrorMessage = $"{field.DisplayName} must be a number",
                                FieldName = fieldName,
                                Value = value
                            };
                        }
                        break;

                    case MetadataFieldType.Select:
                        if (value is string stringValue)
                        {
                            if (!field.Options.Contains(stringValue))
                            {
                                return new MetadataValidationResult
                                {
                                    IsValid = false,
                                    ErrorMessage = $"{field.DisplayName} must be one of: {string.Join(", ", field.Options)}",
                                    FieldName = fieldName,
                                    Value = value
                                };
                            }
                        }
                        break;

                    case MetadataFieldType.TextArea:
                        if (value is string textValue)
                        {
                            if (field.Validation?.MaxLength.HasValue == true && textValue.Length > field.Validation.MaxLength.Value)
                            {
                                return new MetadataValidationResult
                                {
                                    IsValid = false,
                                    ErrorMessage = $"{field.DisplayName} must be at most {field.Validation.MaxLength.Value} characters",
                                    FieldName = fieldName,
                                    Value = value
                                };
                            }
                        }
                        break;
                }

                return new MetadataValidationResult
                {
                    IsValid = true,
                    FieldName = fieldName,
                    Value = value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating field {FieldName}", fieldName);
                return new MetadataValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Validation error: {ex.Message}",
                    FieldName = fieldName,
                    Value = value
                };
            }
        }

        public async Task<object> TransformFieldValueAsync(string fieldName, object value)
        {
            try
            {
                var field = Fields.FirstOrDefault(f => f.Name == fieldName);
                if (field == null)
                {
                    return value;
                }

                // Transform values based on field type
                switch (field.Type)
                {
                    case MetadataFieldType.Number:
                        if (value is string stringValue && double.TryParse(stringValue, out var doubleValue))
                        {
                            return doubleValue;
                        }
                        break;

                    case MetadataFieldType.Boolean:
                        if (value is string boolString)
                        {
                            return boolString.ToLower() == "true" || boolString == "1";
                        }
                        break;
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming field {FieldName}", fieldName);
                return value;
            }
        }

        public async Task<Dictionary<string, object>> GetDefaultValuesAsync()
        {
            return await Task.FromResult(new Dictionary<string, object>
            {
                ["printTime"] = 0.0,
                ["filamentType"] = "PLA",
                ["layerHeight"] = 0.2,
                ["infillPercentage"] = 20,
                ["supports"] = false,
                ["bedTemperature"] = 60,
                ["nozzleTemperature"] = 200,
                ["printSpeed"] = 50,
                ["difficulty"] = "Beginner",
                ["notes"] = ""
            });
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing printing metadata plugin");
            await Task.CompletedTask;
        }

        public async Task UnloadAsync()
        {
            _logger.LogInformation("Unloading printing metadata plugin");
            await Task.CompletedTask;
        }
    }
}
