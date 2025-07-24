using PolyBucket.Api.Common.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace PolyBucket.Api.Features.SystemSettings.Plugins
{
    public class LiquidGlassThemePlugin : IThemePlugin
    {
        public string Id => "liquid-glass-theme-plugin";
        public string Name => "Liquid Glass Theme Plugin";
        public string Version => "1.0.0";
        public string Author => "PolyBucket Team";
        public string Description => "Default liquid glass theme with glassmorphism effects and customizable colors";

        public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>
        {
            new PluginComponent
            {
                Id = "liquid-glass-theme-customizer",
                Name = "Liquid Glass Theme Customizer",
                ComponentPath = "plugins/themes/LiquidGlassThemeCustomizer",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "admin-theme-customization",
                        ComponentId = "liquid-glass-theme-customizer",
                        Priority = 10
                    }
                }
            }
        };

        public PluginMetadata Metadata => new PluginMetadata
        {
            MinimumAppVersion = "1.0.0",
            RequiredPermissions = new List<string>(),
            OptionalPermissions = new List<string> { "admin.theme.customize" },
            Settings = new Dictionary<string, PluginSetting>
            {
                ["enableGlassEffects"] = new PluginSetting
                {
                    Name = "Enable Glass Effects",
                    Description = "Enable backdrop blur and glass morphism effects",
                    Type = PluginSettingType.Boolean,
                    DefaultValue = true
                },
                ["glassBlurAmount"] = new PluginSetting
                {
                    Name = "Glass Blur Amount",
                    Description = "Amount of backdrop blur for glass effects (in pixels)",
                    Type = PluginSettingType.Number,
                    DefaultValue = 20
                }
            },
            Lifecycle = new PluginLifecycle
            {
                AutoStart = true,
                CanDisable = false,
                CanUninstall = false
            }
        };

        public IEnumerable<ThemeDefinition> GetThemes()
        {
            return new List<ThemeDefinition>
            {
                new ThemeDefinition
                {
                    Id = "liquid-glass-default",
                    Name = "Liquid Glass Default",
                    Description = "Default liquid glass theme with purple and blue accents",
                    Author = "PolyBucket Team",
                    Version = "1.0.0",
                    IsDefault = true,
                    IsCustomizable = true,
                    Categories = new List<ThemeCategory>
                    {
                        new ThemeCategory { Id = "colors", Name = "Colors", Order = 1 },
                        new ThemeCategory { Id = "effects", Name = "Effects", Order = 2 },
                        new ThemeCategory { Id = "layout", Name = "Layout", Order = 3 }
                    },
                    Settings = new Dictionary<string, ThemeSetting>
                    {
                        ["primaryColor"] = new ThemeSetting
                        {
                            Name = "Primary Color",
                            Description = "Main brand color",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#6366f1",
                            Required = true
                        },
                        ["primaryLightColor"] = new ThemeSetting
                        {
                            Name = "Primary Light",
                            Description = "Lighter variant for hover states",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#818cf8"
                        },
                        ["primaryDarkColor"] = new ThemeSetting
                        {
                            Name = "Primary Dark",
                            Description = "Darker variant for active states",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#4f46e5"
                        },
                        ["secondaryColor"] = new ThemeSetting
                        {
                            Name = "Secondary Color",
                            Description = "Secondary brand color",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#8b5cf6"
                        },
                        ["secondaryLightColor"] = new ThemeSetting
                        {
                            Name = "Secondary Light",
                            Description = "Lighter secondary variant",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#a78bfa"
                        },
                        ["secondaryDarkColor"] = new ThemeSetting
                        {
                            Name = "Secondary Dark",
                            Description = "Darker secondary variant",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#7c3aed"
                        },
                        ["accentColor"] = new ThemeSetting
                        {
                            Name = "Accent Color",
                            Description = "Accent color for highlights",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#06b6d4"
                        },
                        ["accentLightColor"] = new ThemeSetting
                        {
                            Name = "Accent Light",
                            Description = "Lighter accent variant",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#22d3ee"
                        },
                        ["accentDarkColor"] = new ThemeSetting
                        {
                            Name = "Accent Dark",
                            Description = "Darker accent variant",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#0891b2"
                        },
                        ["backgroundPrimaryColor"] = new ThemeSetting
                        {
                            Name = "Background Primary",
                            Description = "Main background color",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#0f0f23"
                        },
                        ["backgroundSecondaryColor"] = new ThemeSetting
                        {
                            Name = "Background Secondary",
                            Description = "Secondary background color",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#1a1a2e"
                        },
                        ["backgroundTertiaryColor"] = new ThemeSetting
                        {
                            Name = "Background Tertiary",
                            Description = "Tertiary background color",
                            Type = ThemeSettingType.Color,
                            DefaultValue = "#16213e"
                        },
                        ["glassBlurAmount"] = new ThemeSetting
                        {
                            Name = "Glass Blur Amount",
                            Description = "Amount of backdrop blur for glass effects",
                            Type = ThemeSettingType.Number,
                            DefaultValue = 20,
                            MinValue = 0,
                            MaxValue = 50
                        },
                        ["glassOpacity"] = new ThemeSetting
                        {
                            Name = "Glass Opacity",
                            Description = "Opacity of glass elements",
                            Type = ThemeSettingType.Number,
                            DefaultValue = 0.08,
                            MinValue = 0.01,
                            MaxValue = 0.3
                        }
                    },
                    DefaultConfiguration = new Dictionary<string, object>
                    {
                        ["primaryColor"] = "#6366f1",
                        ["primaryLightColor"] = "#818cf8",
                        ["primaryDarkColor"] = "#4f46e5",
                        ["secondaryColor"] = "#8b5cf6",
                        ["secondaryLightColor"] = "#a78bfa",
                        ["secondaryDarkColor"] = "#7c3aed",
                        ["accentColor"] = "#06b6d4",
                        ["accentLightColor"] = "#22d3ee",
                        ["accentDarkColor"] = "#0891b2",
                        ["backgroundPrimaryColor"] = "#0f0f23",
                        ["backgroundSecondaryColor"] = "#1a1a2e",
                        ["backgroundTertiaryColor"] = "#16213e",
                        ["glassBlurAmount"] = 20,
                        ["glassOpacity"] = 0.08
                    }
                }
            };
        }

        public async Task<ThemeComponents> GetThemeComponentsAsync(string themeId)
        {
            if (themeId != "liquid-glass-default")
            {
                throw new ArgumentException($"Unknown theme: {themeId}");
            }

            // In a real implementation, this would load CSS/JS from files
            // For now, we'll return the basic CSS structure
            var css = @"
/* Liquid Glass Theme CSS Variables */
:root {
  --lg-primary: #6366f1;
  --lg-primary-light: #818cf8;
  --lg-primary-dark: #4f46e5;
  --lg-secondary: #8b5cf6;
  --lg-secondary-light: #a78bfa;
  --lg-secondary-dark: #7c3aed;
  --lg-accent: #06b6d4;
  --lg-accent-light: #22d3ee;
  --lg-accent-dark: #0891b2;
  --lg-bg-primary: #0f0f23;
  --lg-bg-secondary: #1a1a2e;
  --lg-bg-tertiary: #16213e;
  --lg-glass-bg: rgba(255, 255, 255, 0.08);
  --lg-glass-border: rgba(255, 255, 255, 0.12);
  --lg-glass-blur: blur(20px);
  --lg-text-primary: #ffffff;
  --lg-text-secondary: rgba(255, 255, 255, 0.8);
  --lg-text-tertiary: rgba(255, 255, 255, 0.6);
  --lg-text-muted: rgba(255, 255, 255, 0.4);
}

/* Base Liquid Glass Styles */
.lg-container {
  background: linear-gradient(135deg, var(--lg-bg-primary) 0%, var(--lg-bg-secondary) 50%, var(--lg-bg-tertiary) 100%);
  min-height: 100vh;
  color: var(--lg-text-primary);
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.lg-card {
  background: var(--lg-glass-bg);
  backdrop-filter: var(--lg-glass-blur);
  -webkit-backdrop-filter: var(--lg-glass-blur);
  border: 1px solid var(--lg-glass-border);
  border-radius: 0.75rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  transition: all 250ms ease-in-out;
}

.lg-button {
  background: var(--lg-glass-bg);
  backdrop-filter: var(--lg-glass-blur);
  -webkit-backdrop-filter: var(--lg-glass-blur);
  border: 1px solid var(--lg-glass-border);
  border-radius: 0.5rem;
  color: var(--lg-text-primary);
  padding: 0.5rem 1.5rem;
  font-weight: 500;
  font-size: 0.875rem;
  transition: all 150ms ease-in-out;
  min-height: 2.5rem;
}

.lg-input {
  background: var(--lg-glass-bg);
  backdrop-filter: var(--lg-glass-blur);
  -webkit-backdrop-filter: var(--lg-glass-blur);
  border: 1px solid var(--lg-glass-border);
  border-radius: 0.5rem;
  color: var(--lg-text-primary);
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  transition: all 150ms ease-in-out;
}
";

            return new ThemeComponents
            {
                Css = css,
                JavaScript = "",
                Variables = new Dictionary<string, object>
                {
                    ["primaryColor"] = "#6366f1",
                    ["primaryLightColor"] = "#818cf8",
                    ["primaryDarkColor"] = "#4f46e5",
                    ["secondaryColor"] = "#8b5cf6",
                    ["secondaryLightColor"] = "#a78bfa",
                    ["secondaryDarkColor"] = "#7c3aed",
                    ["accentColor"] = "#06b6d4",
                    ["accentLightColor"] = "#22d3ee",
                    ["accentDarkColor"] = "#0891b2",
                    ["backgroundPrimaryColor"] = "#0f0f23",
                    ["backgroundSecondaryColor"] = "#1a1a2e",
                    ["backgroundTertiaryColor"] = "#16213e"
                }
            };
        }

        public ThemeValidationResult ValidateThemeConfiguration(string themeId, Dictionary<string, object> configuration)
        {
            if (themeId != "liquid-glass-default")
            {
                return new ThemeValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { $"Unknown theme: {themeId}" }
                };
            }

            var errors = new List<string>();
            var warnings = new List<string>();

            // Validate required color fields
            var requiredColors = new[] { "primaryColor", "secondaryColor", "accentColor", "backgroundPrimaryColor" };
            foreach (var color in requiredColors)
            {
                if (!configuration.ContainsKey(color))
                {
                    errors.Add($"Required color '{color}' is missing");
                }
                else if (!IsValidHexColor(configuration[color]?.ToString()))
                {
                    errors.Add($"Invalid hex color format for '{color}'");
                }
            }

            // Validate numeric fields
            if (configuration.ContainsKey("glassBlurAmount"))
            {
                if (!double.TryParse(configuration["glassBlurAmount"]?.ToString(), out var blurAmount))
                {
                    errors.Add("glassBlurAmount must be a number");
                }
                else if (blurAmount < 0 || blurAmount > 50)
                {
                    errors.Add("glassBlurAmount must be between 0 and 50");
                }
            }

            if (configuration.ContainsKey("glassOpacity"))
            {
                if (!double.TryParse(configuration["glassOpacity"]?.ToString(), out var opacity))
                {
                    errors.Add("glassOpacity must be a number");
                }
                else if (opacity < 0.01 || opacity > 0.3)
                {
                    errors.Add("glassOpacity must be between 0.01 and 0.3");
                }
            }

            return new ThemeValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }

        public async Task<ThemeApplicationResult> ApplyThemeConfigurationAsync(string themeId, Dictionary<string, object> configuration)
        {
            if (themeId != "liquid-glass-default")
            {
                return new ThemeApplicationResult
                {
                    Success = false,
                    Message = $"Unknown theme: {themeId}"
                };
            }

            // Validate configuration first
            var validation = ValidateThemeConfiguration(themeId, configuration);
            if (!validation.IsValid)
            {
                return new ThemeApplicationResult
                {
                    Success = false,
                    Message = "Invalid theme configuration",
                    Warnings = validation.Errors
                };
            }

            // Apply the configuration
            // In a real implementation, this would update the database and cache
            var appliedConfig = new Dictionary<string, object>(configuration);

            return new ThemeApplicationResult
            {
                Success = true,
                Message = "Theme configuration applied successfully",
                AppliedConfiguration = appliedConfig,
                Warnings = validation.Warnings
            };
        }

        public async Task InitializeAsync()
        {
            // Initialize the theme plugin
            await Task.CompletedTask;
        }

        public async Task UnloadAsync()
        {
            // Cleanup when plugin is unloaded
            await Task.CompletedTask;
        }

        private bool IsValidHexColor(string? color)
        {
            if (string.IsNullOrEmpty(color))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
        }
    }
} 