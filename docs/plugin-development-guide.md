# PolyBucket Plugin Development Guide

## Table of Contents

- [Overview](#overview)
- [Plugin Architecture](#plugin-architecture)
- [Plugin Installation & Discovery](#plugin-installation--discovery)
- [Plugin Manifest System](#plugin-manifest-system)
- [Specialized Plugin Types](#specialized-plugin-types)
- [Backend Plugin Development](#backend-plugin-development)
- [Frontend Plugin Development](#frontend-plugin-development)
- [Plugin Examples](#plugin-examples)
- [Plugin CLI & Development Tools](#plugin-cli--development-tools)
- [Plugin Security & Sandboxing](#plugin-security--sandboxing)
- [Plugin Lifecycle](#plugin-lifecycle)
- [Best Practices](#best-practices)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

## Overview

PolyBucket features a comprehensive plugin system that allows developers to extend both the backend API and frontend user interface. The plugin system is designed to be modular, secure, and easy to develop while maintaining the integrity of the core application.

### Key Features

- **Backend Plugins**: Extend API functionality, add new endpoints, integrate with external services
- **Frontend Plugins**: Add new UI components, extend existing pages, create custom workflows
- **Hook System**: Allow plugins to integrate seamlessly with existing application flow
- **Permission Integration**: Plugins work with the existing ACL system
- **Dynamic Loading**: Plugins can be loaded and unloaded without restarting the application
- **Configuration Management**: Plugins can expose settings to administrators
- **Plugin Installation**: Install plugins from GitHub repos, URLs, or marketplace
- **Plugin Discovery**: Browse and install plugins from the PolyBucket marketplace
- **Specialized Types**: Support for themes, OAuth, metadata extensions, and more
- **Plugin Sandboxing**: Secure execution environment for plugin code
- **Development Tools**: CLI tools for plugin development and publishing

## Plugin Architecture

The plugin system consists of several key components:

### Backend Components

1. **IPlugin Interface**: Base interface all backend plugins must implement
2. **PluginManager**: Manages plugin lifecycle, discovery, and loading
3. **Plugin Metadata**: Defines plugin configuration, permissions, and frontend components
4. **MEF Integration**: Uses .NET's Managed Extensibility Framework for plugin discovery

### Frontend Components

1. **PluginRegistry**: Manages frontend plugin registration and component loading
2. **PluginHook**: React component that renders plugin components at specific extension points
3. **Plugin API**: Provides access to application state, services, and functionality
4. **Dynamic Component Loading**: Supports runtime loading of React components

## Plugin Installation & Discovery

### Plugin Installation System

PolyBucket supports multiple ways to install plugins:

#### 1. GitHub Repository Installation
```bash
# Install from GitHub repo
POST /api/plugins/install
{
  "source": "github",
  "url": "https://github.com/user/polybucket-theme-plugin",
  "branch": "main"
}
```

#### 2. Direct URL Installation
```bash
# Install from direct download URL
POST /api/plugins/install
{
  "source": "url",
  "url": "https://example.com/plugin.zip"
}
```

#### 3. Marketplace Installation
```bash
# Install from marketplace
POST /api/plugins/install
{
  "source": "marketplace",
  "pluginId": "dark-theme-plugin",
  "version": "1.2.0"
}
```

### Plugin Installation Service

```csharp
public class PluginInstallationService
{
    public async Task<PluginInstallationResult> InstallFromUrlAsync(string url)
    {
        // Download and validate plugin
        // Extract plugin files
        // Validate manifest
        // Install dependencies
        // Register with plugin manager
    }
    
    public async Task<PluginInstallationResult> InstallFromGitHubAsync(string repoUrl)
    {
        // Clone repository
        // Build plugin if needed
        // Extract plugin files
        // Validate manifest
        // Install dependencies
        // Register with plugin manager
    }
    
    public async Task<PluginInstallationResult> InstallFromMarketplaceAsync(string pluginId, string version)
    {
        // Fetch plugin from marketplace
        // Download plugin package
        // Validate signature
        // Install plugin
        // Register with plugin manager
    }
}
```

### Plugin Discovery

```csharp
[ApiController]
[Route("api/plugins")]
public class PluginManagementController : ControllerBase
{
    [HttpGet("marketplace")]
    public async Task<ActionResult<List<MarketplacePlugin>>> GetMarketplacePlugins()
    {
        var plugins = await _pluginMarketplaceService.GetAvailablePluginsAsync();
        return Ok(plugins);
    }
    
    [HttpPost("install")]
    public async Task<ActionResult<PluginInstallationResult>> InstallPlugin([FromBody] InstallPluginRequest request)
    {
        var result = await _pluginInstallationService.InstallFromUrlAsync(request.Url);
        return Ok(result);
    }
    
    [HttpPost("uninstall/{pluginId}")]
    public async Task<ActionResult> UninstallPlugin(string pluginId)
    {
        await _pluginManager.UninstallPluginAsync(pluginId);
        return Ok();
    }
    
    [HttpGet("installed")]
    public async Task<ActionResult<List<InstalledPlugin>>> GetInstalledPlugins()
    {
        var plugins = await _pluginManager.GetInstalledPluginsAsync();
        return Ok(plugins);
    }
}
```

## Plugin Manifest System

### Plugin Manifest Structure

Every plugin must include a `polybucket-plugin.json` manifest file:

```json
{
  "id": "dark-theme-plugin",
  "name": "Dark Theme",
  "version": "1.2.0",
  "author": "PolyBucket Community",
  "description": "Dark theme for PolyBucket with customizable accent colors",
  "type": "theme",
  "compatibility": {
    "minVersion": "1.0.0",
    "maxVersion": "2.0.0"
  },
  "dependencies": {
    "backend": [],
    "frontend": ["react", "styled-components"]
  },
  "permissions": [
    "theme.modify",
    "ui.customize"
  ],
  "hooks": [
    {
      "name": "theme-override",
      "component": "DarkThemeProvider",
      "priority": 100
    }
  ],
  "settings": [
    {
      "key": "accentColor",
      "type": "color",
      "default": "#007bff",
      "label": "Accent Color",
      "description": "Primary accent color for the theme"
    },
    {
      "key": "enableAnimations",
      "type": "boolean",
      "default": true,
      "label": "Enable Animations"
    }
  ],
  "assets": {
    "css": ["styles/dark-theme.css"],
    "js": ["scripts/theme-utils.js"],
    "images": ["icons/theme-icon.png"]
  },
  "repository": {
    "type": "git",
    "url": "https://github.com/user/polybucket-theme-plugin"
  },
  "keywords": ["theme", "dark", "ui", "customization"],
  "license": "MIT"
}
```

### Manifest Validation

```csharp
public class PluginManifestValidator
{
    public async Task<ValidationResult> ValidateManifestAsync(string manifestPath)
    {
        var manifest = await JsonSerializer.DeserializeAsync<PluginManifest>(manifestPath);
        
        // Validate required fields
        if (string.IsNullOrEmpty(manifest.Id))
            return ValidationResult.Failed("Plugin ID is required");
            
        // Validate version format
        if (!Version.TryParse(manifest.Version, out _))
            return ValidationResult.Failed("Invalid version format");
            
        // Validate compatibility
        if (!IsCompatible(manifest.Compatibility))
            return ValidationResult.Failed("Plugin not compatible with current version");
            
        return ValidationResult.Success();
    }
}
```

## Specialized Plugin Types

### Theme Plugins

```csharp
public interface IThemePlugin : IPlugin
{
    string ThemeName { get; }
    Dictionary<string, string> CSSVariables { get; }
    Dictionary<string, object> ComponentOverrides { get; }
    Task ApplyThemeAsync();
    Task RemoveThemeAsync();
}

public class DarkThemePlugin : IThemePlugin
{
    public string ThemeName => "Dark Theme";
    
    public Dictionary<string, string> CSSVariables => new()
    {
        ["--primary-color"] = "#007bff",
        ["--background-color"] = "#1a1a1a",
        ["--text-color"] = "#ffffff"
    };
    
    public async Task ApplyThemeAsync()
    {
        // Apply theme variables
        // Override component styles
        // Update UI elements
    }
}
```

### OAuth Plugins

```csharp
public interface IOAuthPlugin : IPlugin
{
    string ProviderName { get; }
    string ClientId { get; }
    string ClientSecret { get; }
    string AuthorizationUrl { get; }
    string TokenUrl { get; }
    string UserInfoUrl { get; }
    
    Task<OAuthResult> AuthenticateAsync(string code);
    Task<UserInfo> GetUserInfoAsync(string accessToken);
    Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken);
}

public class DiscordOAuthPlugin : IOAuthPlugin
{
    public string ProviderName => "Discord";
    public string ClientId => _configuration["Discord:ClientId"];
    public string ClientSecret => _configuration["Discord:ClientSecret"];
    public string AuthorizationUrl => "https://discord.com/api/oauth2/authorize";
    public string TokenUrl => "https://discord.com/api/oauth2/token";
    public string UserInfoUrl => "https://discord.com/api/users/@me";
    
    public async Task<OAuthResult> AuthenticateAsync(string code)
    {
        // Exchange code for access token
        // Return OAuth result
    }
}
```

### Metadata Extension Plugins

```csharp
public interface IMetadataPlugin : IPlugin
{
    IEnumerable<MetadataField> GetModelFields();
    IEnumerable<MetadataField> GetUserFields();
    IEnumerable<MetadataField> GetCollectionFields();
    
    Task<object> ProcessMetadataAsync(string entityType, Guid entityId, object metadata);
    Task<ValidationResult> ValidateMetadataAsync(string entityType, object metadata);
}

public class PrintingMetadataPlugin : IMetadataPlugin
{
    public IEnumerable<MetadataField> GetModelFields()
    {
        return new[]
        {
            new MetadataField
            {
                Name = "LayerHeight",
                Type = "number",
                Label = "Layer Height (mm)",
                Description = "Recommended layer height for 3D printing"
            },
            new MetadataField
            {
                Name = "InfillPercentage",
                Type = "number",
                Label = "Infill Percentage",
                Description = "Recommended infill percentage"
            },
            new MetadataField
            {
                Name = "Supports",
                Type = "boolean",
                Label = "Requires Supports",
                Description = "Whether the model requires support structures"
            }
        };
    }
}
```

### Localization Plugins

```csharp
public interface ILocalizationPlugin : IPlugin
{
    string Language { get; }
    string LanguageCode { get; }
    Dictionary<string, string> Translations { get; }
    string DateFormat { get; }
    string NumberFormat { get; }
    string Currency { get; }
    
    Task<string> TranslateAsync(string key, params object[] args);
    Task<Dictionary<string, string>> GetTranslationsAsync();
}

public class SpanishLocalizationPlugin : ILocalizationPlugin
{
    public string Language => "Spanish";
    public string LanguageCode => "es";
    
    public Dictionary<string, string> Translations => new()
    {
        ["models.title"] = "Modelos",
        ["models.download"] = "Descargar",
        ["models.like"] = "Me gusta",
        ["collections.title"] = "Colecciones"
    };
}
```

## Backend Plugin Development

### 1. Creating a Basic Plugin

Every backend plugin must implement the `IPlugin` interface:

```csharp
using PolyBucket.Api.Common.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourNamespace.Plugins
{
    public class MyPlugin : IPlugin
    {
        public string Id => "my-plugin";
        public string Name => "My Plugin";
        public string Version => "1.0.0";
        public string Author => "Your Name";
        public string Description => "Description of what your plugin does";

        public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>
        {
            // Define frontend components here
        };

        public PluginMetadata Metadata => new PluginMetadata
        {
            MinimumAppVersion = "1.0.0",
            RequiredPermissions = new List<string> { "permission.required" },
            OptionalPermissions = new List<string> { "permission.optional" },
            Settings = new Dictionary<string, PluginSetting>(),
            Lifecycle = new PluginLifecycle
            {
                AutoStart = true,
                CanDisable = true,
                CanUninstall = true
            }
        };

        public async Task InitializeAsync()
        {
            // Plugin initialization logic
            await Task.CompletedTask;
        }

        public async Task UnloadAsync()
        {
            // Plugin cleanup logic
            await Task.CompletedTask;
        }
    }
}
```

### 2. Plugin with Database Integration

For plugins that need database access, inject the DbContext:

```csharp
public class DatabasePlugin : IPlugin
{
    private readonly PolyBucketDbContext _context;

    public DatabasePlugin(PolyBucketDbContext context)
    {
        _context = context;
    }

    // ... implement interface members

    public async Task<IEnumerable<MyEntity>> GetMyEntitiesAsync()
    {
        return await _context.Set<MyEntity>()
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }
}
```

### 3. Creating Plugin-Specific Domain Models

Add your entities to the plugin namespace:

```csharp
namespace YourNamespace.Plugins.Domain
{
    public class MyEntity : Auditable
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public virtual User Owner { get; set; } = null!;
    }
}
```

### 4. Plugin Settings Configuration

Define configurable settings for your plugin:

```csharp
public PluginMetadata Metadata => new PluginMetadata
{
    Settings = new Dictionary<string, PluginSetting>
    {
        ["apiKey"] = new PluginSetting
        {
            Name = "API Key",
            Description = "External service API key",
            Type = PluginSettingType.String,
            Required = true
        },
        ["enableFeature"] = new PluginSetting
        {
            Name = "Enable Feature",
            Description = "Enable the advanced feature",
            Type = PluginSettingType.Boolean,
            DefaultValue = false,
            Required = false
        },
        ["maxItems"] = new PluginSetting
        {
            Name = "Max Items",
            Description = "Maximum number of items to process",
            Type = PluginSettingType.Number,
            DefaultValue = 100,
            Required = true
        }
    }
};
```

### 5. Creating Plugin Controllers

Add API endpoints specific to your plugin:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace YourNamespace.Plugins.Http
{
    [ApiController]
    [Route("api/myplugin")]
    [Authorize]
    public class MyPluginController : ControllerBase
    {
        private readonly IMyPlugin _plugin;

        public MyPluginController(IMyPlugin plugin)
        {
            _plugin = plugin;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _plugin.GetItemsAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found"));

            var item = await _plugin.CreateItemAsync(userId, request);
            return Ok(item);
        }
    }
}
```

## Frontend Plugin Development

### 1. Plugin Registration

Frontend plugins are automatically registered when the backend plugin provides frontend component metadata:

```csharp
public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>
{
    new PluginComponent
    {
        Id = "my-widget",
        Name = "My Widget",
        ComponentPath = "plugins/myplugin/MyWidget",
        Type = ComponentType.Widget,
        Hooks = new List<PluginHook>
        {
            new PluginHook
            {
                HookName = "dashboard-sidebar",
                ComponentId = "my-widget",
                Priority = 50
            }
        }
    }
};
```

### 2. Creating Plugin Components

Create React components in the specified path:

```typescript
// frontend/src/plugins/myplugin/MyWidget.tsx
import React, { useState, useEffect } from 'react';
import { usePlugin, PluginComponentProps } from '../PluginAPI';

interface MyWidgetProps extends PluginComponentProps {
    title?: string;
    maxItems?: number;
}

const MyWidget: React.FC<MyWidgetProps> = ({
    pluginId,
    componentId,
    title = "My Widget",
    maxItems = 10
}) => {
    const plugin = usePlugin(pluginId);
    const [items, setItems] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadItems();
    }, []);

    const loadItems = async () => {
        try {
            setLoading(true);
            const response = await plugin.api.makeRequest('/myplugin');
            if (response.ok) {
                const data = await response.json();
                setItems(data.slice(0, maxItems));
            }
        } catch (error) {
            plugin.showNotification('Failed to load items', 'error');
        } finally {
            setLoading(false);
        }
    };

    const handleCreateItem = async () => {
        try {
            const name = prompt('Enter item name:');
            if (!name) return;

            const response = await plugin.api.makeRequest('/myplugin', {
                method: 'POST',
                body: JSON.stringify({ name })
            });

            if (response.ok) {
                await loadItems();
                plugin.showNotification('Item created successfully', 'success');
            }
        } catch (error) {
            plugin.showNotification('Failed to create item', 'error');
        }
    };

    if (loading) {
        return (
            <div className="p-4">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    return (
        <div className="plugin-widget p-4 border rounded-md">
            <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-semibold">{title}</h3>
                <button
                    onClick={handleCreateItem}
                    className="px-3 py-1 bg-blue-500 text-white rounded text-sm hover:bg-blue-600"
                >
                    Add Item
                </button>
            </div>
            
            <div className="space-y-2">
                {items.length === 0 ? (
                    <p className="text-gray-500">No items found</p>
                ) : (
                    items.map((item, index) => (
                        <div key={index} className="p-2 bg-gray-50 rounded">
                            {item.name}
                        </div>
                    ))
                )}
            </div>
        </div>
    );
};

export default MyWidget;
```

### 3. Using Plugin Hooks

Add your plugin components to existing pages using hooks:

```typescript
// In an existing page component
import PluginHook from '../plugins/PluginHook';

const Dashboard: React.FC = () => {
    return (
        <div className="dashboard">
            <div className="main-content">
                {/* Existing dashboard content */}
            </div>
            
            <div className="sidebar">
                {/* Render all plugins registered to this hook */}
                <PluginHook 
                    hookName="dashboard-sidebar"
                    context={{ userId: currentUser?.id }}
                    className="plugin-widgets space-y-4"
                />
            </div>
        </div>
    );
};
```

### 4. Plugin API Usage

The Plugin API provides access to application functionality:

```typescript
const MyComponent: React.FC<PluginComponentProps> = ({ pluginId }) => {
    const plugin = usePlugin(pluginId);

    // Access application state
    const currentUser = plugin.getCurrentUser();
    const hasPermission = plugin.hasPermission('my.permission');

    // Use plugin-specific storage
    const saveData = () => {
        plugin.storage.set('myData', { value: 'example' });
    };

    const loadData = () => {
        return plugin.storage.get('myData');
    };

    // Make API requests
    const apiCall = async () => {
        const response = await plugin.api.makeRequest('/my-endpoint');
        return response.json();
    };

    // Show notifications
    const notify = () => {
        plugin.showNotification('Hello from plugin!', 'info');
    };

    // Access services
    const { models, collections } = plugin.services;

    return (
        <div>
            <p>User: {currentUser?.username}</p>
            <p>Has Permission: {hasPermission ? 'Yes' : 'No'}</p>
            <button onClick={notify}>Show Notification</button>
        </div>
    );
};
```

## Plugin Examples

### Theme Plugin Example

#### Dark Theme Plugin

```csharp
// Backend: DarkThemePlugin.cs
public class DarkThemePlugin : IThemePlugin
{
    public string Id => "dark-theme-plugin";
    public string Name => "Dark Theme";
    public string Version => "1.2.0";
    public string Author => "PolyBucket Community";
    public string Description => "Dark theme with customizable accent colors";
    
    public string ThemeName => "Dark Theme";
    
    public Dictionary<string, string> CSSVariables => new()
    {
        ["--primary-color"] = "#007bff",
        ["--secondary-color"] = "#6c757d",
        ["--background-color"] = "#1a1a1a",
        ["--surface-color"] = "#2d2d2d",
        ["--text-color"] = "#ffffff",
        ["--text-muted"] = "#b0b0b0",
        ["--border-color"] = "#404040",
        ["--shadow-color"] = "rgba(0, 0, 0, 0.3)"
    };
    
    public async Task ApplyThemeAsync()
    {
        // Apply CSS variables to document
        // Override component styles
        // Update theme context
    }
    
    public async Task RemoveThemeAsync()
    {
        // Remove theme variables
        // Restore default styles
        // Clear theme context
    }
}
```

```typescript
// Frontend: DarkThemeProvider.tsx
import React, { createContext, useContext, useEffect } from 'react';

interface DarkThemeContextType {
  isDark: boolean;
  toggleTheme: () => void;
  accentColor: string;
  setAccentColor: (color: string) => void;
}

const DarkThemeContext = createContext<DarkThemeContextType | undefined>(undefined);

export const DarkThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isDark, setIsDark] = React.useState(false);
  const [accentColor, setAccentColor] = React.useState('#007bff');

  useEffect(() => {
    if (isDark) {
      document.documentElement.style.setProperty('--primary-color', accentColor);
      document.documentElement.style.setProperty('--background-color', '#1a1a1a');
      document.documentElement.style.setProperty('--text-color', '#ffffff');
    } else {
      document.documentElement.style.removeProperty('--primary-color');
      document.documentElement.style.removeProperty('--background-color');
      document.documentElement.style.removeProperty('--text-color');
    }
  }, [isDark, accentColor]);

  const toggleTheme = () => setIsDark(!isDark);

  return (
    <DarkThemeContext.Provider value={{ isDark, toggleTheme, accentColor, setAccentColor }}>
      {children}
    </DarkThemeContext.Provider>
  );
};

export const useDarkTheme = () => {
  const context = useContext(DarkThemeContext);
  if (!context) {
    throw new Error('useDarkTheme must be used within DarkThemeProvider');
  }
  return context;
};
```

### OAuth Plugin Example

#### Discord OAuth Plugin

```csharp
// Backend: DiscordOAuthPlugin.cs
public class DiscordOAuthPlugin : IOAuthPlugin
{
    public string Id => "discord-oauth-plugin";
    public string Name => "Discord OAuth";
    public string Version => "1.0.0";
    public string Author => "PolyBucket Community";
    public string Description => "Discord account integration and community features";
    
    public string ProviderName => "Discord";
    public string ClientId => _configuration["Discord:ClientId"];
    public string ClientSecret => _configuration["Discord:ClientSecret"];
    public string AuthorizationUrl => "https://discord.com/api/oauth2/authorize";
    public string TokenUrl => "https://discord.com/api/oauth2/token";
    public string UserInfoUrl => "https://discord.com/api/users/@me";
    
    public async Task<OAuthResult> AuthenticateAsync(string code)
    {
        var tokenRequest = new
        {
            client_id = ClientId,
            client_secret = ClientSecret,
            grant_type = "authorization_code",
            code = code,
            redirect_uri = _configuration["Discord:RedirectUri"]
        };
        
        var response = await _httpClient.PostAsJsonAsync(TokenUrl, tokenRequest);
        var tokenData = await response.Content.ReadFromJsonAsync<DiscordTokenResponse>();
        
        return new OAuthResult
        {
            AccessToken = tokenData.AccessToken,
            RefreshToken = tokenData.RefreshToken,
            ExpiresIn = tokenData.ExpiresIn,
            Scope = tokenData.Scope
        };
    }
    
    public async Task<UserInfo> GetUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
            
        var response = await _httpClient.GetAsync(UserInfoUrl);
        var userData = await response.Content.ReadFromJsonAsync<DiscordUserResponse>();
        
        return new UserInfo
        {
            Id = userData.Id,
            Username = userData.Username,
            DisplayName = userData.GlobalName ?? userData.Username,
            Email = userData.Email,
            AvatarUrl = userData.Avatar != null 
                ? $"https://cdn.discordapp.com/avatars/{userData.Id}/{userData.Avatar}.png"
                : null
        };
    }
}
```

```typescript
// Frontend: DiscordIntegration.tsx
import React, { useState, useEffect } from 'react';
import { usePlugin } from '../PluginAPI';

const DiscordIntegration: React.FC = () => {
  const plugin = usePlugin('discord-oauth-plugin');
  const [isConnected, setIsConnected] = useState(false);
  const [discordUser, setDiscordUser] = useState(null);

  useEffect(() => {
    checkConnectionStatus();
  }, []);

  const checkConnectionStatus = async () => {
    try {
      const response = await plugin.api.makeRequest('/api/oauth/discord/status');
      if (response.ok) {
        const data = await response.json();
        setIsConnected(data.connected);
        setDiscordUser(data.user);
      }
    } catch (error) {
      console.error('Failed to check Discord connection:', error);
    }
  };

  const connectDiscord = async () => {
    try {
      const response = await plugin.api.makeRequest('/api/oauth/discord/authorize');
      if (response.ok) {
        const data = await response.json();
        window.location.href = data.authorizationUrl;
      }
    } catch (error) {
      plugin.showNotification('Failed to connect Discord', 'error');
    }
  };

  const disconnectDiscord = async () => {
    try {
      await plugin.api.makeRequest('/api/oauth/discord/disconnect', {
        method: 'POST'
      });
      setIsConnected(false);
      setDiscordUser(null);
      plugin.showNotification('Discord disconnected', 'success');
    } catch (error) {
      plugin.showNotification('Failed to disconnect Discord', 'error');
    }
  };

  return (
    <div className="discord-integration p-4 border rounded-md">
      <h3 className="text-lg font-semibold mb-4">Discord Integration</h3>
      
      {isConnected ? (
        <div className="space-y-2">
          <p className="text-green-600">Connected to Discord</p>
          {discordUser && (
            <div className="flex items-center space-x-2">
              <img 
                src={discordUser.avatarUrl} 
                alt={discordUser.username}
                className="w-8 h-8 rounded-full"
              />
              <span>{discordUser.displayName}</span>
            </div>
          )}
          <button 
            onClick={disconnectDiscord}
            className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
          >
            Disconnect
          </button>
        </div>
      ) : (
        <button 
          onClick={connectDiscord}
          className="px-4 py-2 bg-indigo-500 text-white rounded hover:bg-indigo-600"
        >
          Connect Discord
        </button>
      )}
    </div>
  );
};

export default DiscordIntegration;
```

### Metadata Extension Plugin Example

#### 3D Printing Metadata Plugin

```csharp
// Backend: PrintingMetadataPlugin.cs
public class PrintingMetadataPlugin : IMetadataPlugin
{
    public string Id => "printing-metadata-plugin";
    public string Name => "3D Printing Metadata";
    public string Version => "1.0.0";
    public string Author => "PolyBucket Community";
    public string Description => "Adds 3D printing specific metadata to models";
    
    public IEnumerable<MetadataField> GetModelFields()
    {
        return new[]
        {
            new MetadataField
            {
                Name = "LayerHeight",
                Type = "number",
                Label = "Layer Height (mm)",
                Description = "Recommended layer height for 3D printing",
                MinValue = 0.1,
                MaxValue = 0.5,
                DefaultValue = 0.2
            },
            new MetadataField
            {
                Name = "InfillPercentage",
                Type = "number",
                Label = "Infill Percentage",
                Description = "Recommended infill percentage",
                MinValue = 0,
                MaxValue = 100,
                DefaultValue = 20
            },
            new MetadataField
            {
                Name = "Supports",
                Type = "boolean",
                Label = "Requires Supports",
                Description = "Whether the model requires support structures",
                DefaultValue = false
            },
            new MetadataField
            {
                Name = "PrintTime",
                Type = "number",
                Label = "Estimated Print Time (hours)",
                Description = "Estimated printing time",
                MinValue = 0,
                DefaultValue = 1
            },
            new MetadataField
            {
                Name = "Material",
                Type = "select",
                Label = "Recommended Material",
                Description = "Recommended printing material",
                Options = new[] { "PLA", "ABS", "PETG", "TPU", "Wood", "Metal" },
                DefaultValue = "PLA"
            }
        };
    }
    
    public async Task<object> ProcessMetadataAsync(string entityType, Guid entityId, object metadata)
    {
        if (entityType != "model") return metadata;
        
        // Process and validate printing metadata
        var printingData = JsonSerializer.Deserialize<PrintingMetadata>(metadata.ToString());
        
        // Calculate derived values
        if (printingData.LayerHeight > 0 && printingData.InfillPercentage > 0)
        {
            printingData.EstimatedFilament = CalculateFilamentUsage(printingData);
        }
        
        return printingData;
    }
}
```

```typescript
// Frontend: PrintingMetadataForm.tsx
import React, { useState, useEffect } from 'react';
import { usePlugin } from '../PluginAPI';

interface PrintingMetadata {
  layerHeight: number;
  infillPercentage: number;
  supports: boolean;
  printTime: number;
  material: string;
}

const PrintingMetadataForm: React.FC<{ modelId: string }> = ({ modelId }) => {
  const plugin = usePlugin('printing-metadata-plugin');
  const [metadata, setMetadata] = useState<PrintingMetadata>({
    layerHeight: 0.2,
    infillPercentage: 20,
    supports: false,
    printTime: 1,
    material: 'PLA'
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadMetadata();
  }, [modelId]);

  const loadMetadata = async () => {
    try {
      const response = await plugin.api.makeRequest(`/api/models/${modelId}/metadata/printing`);
      if (response.ok) {
        const data = await response.json();
        setMetadata(data);
      }
    } catch (error) {
      console.error('Failed to load printing metadata:', error);
    }
  };

  const saveMetadata = async () => {
    try {
      setLoading(true);
      await plugin.api.makeRequest(`/api/models/${modelId}/metadata/printing`, {
        method: 'PUT',
        body: JSON.stringify(metadata)
      });
      plugin.showNotification('Printing metadata saved', 'success');
    } catch (error) {
      plugin.showNotification('Failed to save printing metadata', 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="printing-metadata p-4 border rounded-md">
      <h3 className="text-lg font-semibold mb-4">3D Printing Settings</h3>
      
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">Layer Height (mm)</label>
          <input
            type="number"
            step="0.1"
            min="0.1"
            max="0.5"
            value={metadata.layerHeight}
            onChange={(e) => setMetadata({...metadata, layerHeight: parseFloat(e.target.value)})}
            className="w-full px-3 py-2 border rounded-md"
          />
        </div>
        
        <div>
          <label className="block text-sm font-medium mb-1">Infill Percentage</label>
          <input
            type="number"
            min="0"
            max="100"
            value={metadata.infillPercentage}
            onChange={(e) => setMetadata({...metadata, infillPercentage: parseInt(e.target.value)})}
            className="w-full px-3 py-2 border rounded-md"
          />
        </div>
        
        <div>
          <label className="block text-sm font-medium mb-1">Recommended Material</label>
          <select
            value={metadata.material}
            onChange={(e) => setMetadata({...metadata, material: e.target.value})}
            className="w-full px-3 py-2 border rounded-md"
          >
            <option value="PLA">PLA</option>
            <option value="ABS">ABS</option>
            <option value="PETG">PETG</option>
            <option value="TPU">TPU</option>
            <option value="Wood">Wood</option>
            <option value="Metal">Metal</option>
          </select>
        </div>
        
        <div className="flex items-center">
          <input
            type="checkbox"
            id="supports"
            checked={metadata.supports}
            onChange={(e) => setMetadata({...metadata, supports: e.target.checked})}
            className="mr-2"
          />
          <label htmlFor="supports">Requires Supports</label>
        </div>
        
        <button
          onClick={saveMetadata}
          disabled={loading}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:opacity-50"
        >
          {loading ? 'Saving...' : 'Save Settings'}
        </button>
      </div>
    </div>
  );
};

export default PrintingMetadataForm;
```

## Comment Plugin Example

The enhanced comment plugin serves as a comprehensive example of the plugin system. Here's how it's structured:

### Backend Implementation

```csharp
// Domain Models
public class EnhancedComment : Auditable
{
    public string Content { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public CommentTargetType TargetType { get; set; }
    public Guid AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    // ... additional properties
}

// Plugin Interface
public interface IEnhancedCommentsPlugin : IPlugin
{
    Task<EnhancedComment> AddCommentAsync(CommentTarget target, Guid authorId, string content, Guid? parentCommentId = null);
    Task<IEnumerable<EnhancedComment>> GetCommentsForTargetAsync(CommentTarget target, bool includeHidden = false, int page = 1, int pageSize = 20);
    // ... additional methods
}

// Plugin Implementation
public class EnhancedCommentsPlugin : IEnhancedCommentsPlugin
{
    private readonly PolyBucketDbContext _context;
    private readonly IReportingPlugin _reportingPlugin;

    public EnhancedCommentsPlugin(PolyBucketDbContext context, IReportingPlugin reportingPlugin)
    {
        _context = context;
        _reportingPlugin = reportingPlugin;
    }

    // ... implementation
}
```

### Frontend Implementation

```typescript
// Comment Widget Component
const EnhancedCommentsWidget: React.FC<EnhancedCommentsWidgetProps> = ({
    pluginId,
    targetId,
    targetType,
    readonly = false
}) => {
    const plugin = usePlugin(pluginId);
    const [comments, setComments] = useState<Comment[]>([]);

    // Load comments, handle interactions, etc.
    // ... component implementation
};

export default EnhancedCommentsWidget;
```

### Hook Integration

The comment plugin registers hooks for multiple extension points:

```csharp
Hooks = new List<PluginHook>
{
    new PluginHook
    {
        HookName = "model-details-sidebar",
        ComponentId = "enhanced-comments-widget",
        Priority = 50,
        Config = new Dictionary<string, object> { { "targetType", "model" } }
    },
    new PluginHook
    {
        HookName = "user-profile-tabs",
        ComponentId = "enhanced-comments-widget",
        Priority = 30,
        Config = new Dictionary<string, object> { { "targetType", "user" } }
    }
}
```

## Plugin CLI & Development Tools

### Plugin CLI Tool

The PolyBucket CLI provides tools for plugin development, building, and publishing:

```bash
# Install the PolyBucket CLI
npm install -g @polybucket/cli

# Create a new plugin
polybucket-plugin create my-theme-plugin --type=theme

# Initialize plugin in existing directory
polybucket-plugin init

# Build plugin for production
polybucket-plugin build

# Test plugin locally
polybucket-plugin dev

# Publish plugin to marketplace
polybucket-plugin publish

# Install plugin from source
polybucket-plugin install https://github.com/user/plugin-repo

# List installed plugins
polybucket-plugin list

# Update plugin
polybucket-plugin update my-plugin

# Remove plugin
polybucket-plugin remove my-plugin
```

### Plugin Development Workflow

```bash
# 1. Create new plugin
polybucket-plugin create my-plugin --type=theme

# 2. Start development server
polybucket-plugin dev

# 3. Make changes to plugin code
# - Backend changes require server restart
# - Frontend changes hot-reload automatically

# 4. Test plugin functionality
# - Plugin runs in isolated environment
# - Access to development APIs
# - Debug logging enabled

# 5. Build for production
polybucket-plugin build

# 6. Test production build
polybucket-plugin test

# 7. Publish to marketplace
polybucket-plugin publish
```

### Plugin Development Kit

The Plugin Development Kit (PDK) provides tools and utilities for plugin development:

```typescript
// Plugin Development Kit
import { PluginSDK } from '@polybucket/plugin-sdk';

const sdk = new PluginSDK({
  pluginId: 'my-plugin',
  version: '1.0.0',
  environment: 'development'
});

// Access to development APIs
const models = await sdk.api.models.getAll();
const users = await sdk.api.users.getCurrent();

// Plugin-specific storage
await sdk.storage.set('myData', { value: 'example' });
const data = await sdk.storage.get('myData');

// Development utilities
sdk.logger.info('Plugin initialized');
sdk.notifications.show('Development mode active', 'info');
```

### Plugin Testing Framework

```typescript
// Plugin test setup
import { PluginTestRunner } from '@polybucket/plugin-testing';

describe('My Plugin', () => {
  let testRunner: PluginTestRunner;
  
  beforeEach(async () => {
    testRunner = new PluginTestRunner({
      pluginId: 'my-plugin',
      mockData: {
        models: [...],
        users: [...]
      }
    });
    
    await testRunner.initialize();
  });
  
  afterEach(async () => {
    await testRunner.cleanup();
  });
  
  it('should apply theme correctly', async () => {
    const theme = await testRunner.plugin.applyTheme();
    expect(theme.isApplied).toBe(true);
  });
  
  it('should handle user interactions', async () => {
    const result = await testRunner.simulateUserAction('click', '.theme-toggle');
    expect(result.success).toBe(true);
  });
});
```

## Plugin Security & Sandboxing

### Plugin Sandboxing

Plugins run in isolated environments to prevent security issues:

```typescript
// Frontend Plugin Sandbox
const PluginSandbox: React.FC<PluginSandboxProps> = ({ 
  plugin, 
  children 
}) => {
  return (
    <ErrorBoundary fallback={<PluginErrorFallback />}>
      <SecurityContext.Provider value={plugin.permissions}>
        <CSPProvider policy={plugin.contentSecurityPolicy}>
          <IsolatedScope>
            {children}
          </IsolatedScope>
        </CSPProvider>
      </SecurityContext.Provider>
    </ErrorBoundary>
  );
};
```

### Content Security Policy

```typescript
// Plugin CSP Configuration
const pluginCSP = {
  'default-src': ["'self'"],
  'script-src': ["'self'", "'unsafe-inline'"],
  'style-src': ["'self'", "'unsafe-inline'"],
  'img-src': ["'self'", "data:", "https:"],
  'connect-src': ["'self'", "https://api.polybucket.com"],
  'frame-src': ["'none'"],
  'object-src': ["'none'"],
  'base-uri': ["'self'"],
  'form-action': ["'self'"]
};
```

### Permission System

```csharp
// Backend Permission Validation
public class PluginPermissionValidator
{
    public async Task<bool> ValidatePermissionAsync(string pluginId, string permission, string resource)
    {
        var plugin = await _pluginManager.GetPluginAsync(pluginId);
        var user = await _userService.GetCurrentUserAsync();
        
        // Check plugin permissions
        if (!plugin.Metadata.RequiredPermissions.Contains(permission))
            return false;
            
        // Check user permissions
        if (!await _permissionService.HasPermissionAsync(user.Id, permission))
            return false;
            
        // Check resource access
        if (!await _resourceService.CanAccessAsync(user.Id, resource))
            return false;
            
        return true;
    }
}
```

### Plugin Isolation

```csharp
// Backend Plugin Isolation
public class PluginIsolationService
{
    public async Task<T> ExecuteInIsolationAsync<T>(string pluginId, Func<Task<T>> action)
    {
        using var scope = _serviceProvider.CreateScope();
        var pluginContext = new PluginExecutionContext(pluginId, scope.ServiceProvider);
        
        try
        {
            // Set up isolated environment
            await _setupIsolatedEnvironment(pluginContext);
            
            // Execute plugin code
            return await action();
        }
        catch (Exception ex)
        {
            // Log error and prevent plugin crash from affecting main app
            _logger.LogError(ex, "Plugin {PluginId} execution failed", pluginId);
            throw new PluginExecutionException($"Plugin execution failed: {ex.Message}", ex);
        }
        finally
        {
            // Clean up isolated environment
            await _cleanupIsolatedEnvironment(pluginContext);
        }
    }
}
```

### Security Monitoring

```csharp
// Plugin Security Monitoring
public class PluginSecurityMonitor
{
    public async Task MonitorPluginActivityAsync(string pluginId)
    {
        var plugin = await _pluginManager.GetPluginAsync(pluginId);
        
        // Monitor API calls
        _apiCallMonitor.Track(pluginId, (endpoint, method) => {
            if (IsSuspiciousActivity(endpoint, method))
            {
                _securityLogger.LogWarning("Suspicious activity detected in plugin {PluginId}: {Endpoint} {Method}", 
                    pluginId, endpoint, method);
            }
        });
        
        // Monitor resource usage
        _resourceMonitor.Track(pluginId, (cpu, memory, disk) => {
            if (IsExcessiveResourceUsage(cpu, memory, disk))
            {
                _securityLogger.LogWarning("Excessive resource usage in plugin {PluginId}: CPU={CPU}%, Memory={Memory}MB", 
                    pluginId, cpu, memory);
            }
        });
        
        // Monitor network activity
        _networkMonitor.Track(pluginId, (url, method) => {
            if (IsUnauthorizedNetworkAccess(url, method))
            {
                _securityLogger.LogError("Unauthorized network access in plugin {PluginId}: {Url} {Method}", 
                    pluginId, url, method);
            }
        });
    }
}
```

## Plugin Lifecycle

### 1. Registration

Plugins are registered through dependency injection in `Program.cs`:

```csharp
// Register the plugin service
builder.Services.AddScoped<IMyPlugin, MyPlugin>();

// Plugin manager automatically discovers plugins through MEF
var pluginManager = app.Services.GetRequiredService<PluginManager>();
await pluginManager.InitializeAsync();
```

### 2. Initialization

When the plugin manager initializes:

1. Discovers all classes implementing `IPlugin`
2. Calls `InitializeAsync()` on each plugin
3. Registers frontend components with the frontend registry
4. Sets up plugin-specific routes and controllers

### 3. Runtime Operations

During runtime:

1. Plugin methods are called through dependency injection
2. Frontend components are dynamically loaded when needed
3. Hook components are rendered at their registered extension points
4. Plugin settings can be modified through the admin interface

### 4. Shutdown

When the application shuts down:

1. `UnloadAsync()` is called on all plugins
2. Plugin resources are cleaned up
3. Database connections are closed

## Best Practices

### Backend Development

1. **Use Dependency Injection**: Leverage ASP.NET Core's DI container for all dependencies
2. **Follow CQRS Pattern**: Separate commands and queries into discrete operations
3. **Implement Proper Error Handling**: Use try-catch blocks and return appropriate error responses
4. **Add Logging**: Use ILogger for debugging and monitoring
5. **Validate Input**: Always validate user input and API requests
6. **Use Transactions**: Wrap database operations in transactions when needed
7. **Test Thoroughly**: Write unit and integration tests for your plugin

### Frontend Development

1. **Use TypeScript**: Leverage type safety for better development experience
2. **Handle Loading States**: Always show loading indicators during async operations
3. **Error Boundaries**: Implement error boundaries to prevent plugin crashes from affecting the main app
4. **Responsive Design**: Ensure your components work on all screen sizes
5. **Accessibility**: Follow accessibility guidelines for inclusive design
6. **Performance**: Optimize for performance, especially when handling large datasets
7. **Clean Up**: Remove event listeners and cancel requests in useEffect cleanup

### Security Considerations

1. **Permission Checks**: Always verify user permissions before performing actions
2. **Input Sanitization**: Sanitize and validate all user inputs
3. **SQL Injection Prevention**: Use parameterized queries and Entity Framework
4. **XSS Prevention**: Escape user content in frontend components
5. **CSRF Protection**: Use anti-forgery tokens for state-changing operations
6. **Rate Limiting**: Implement rate limiting for API endpoints
7. **Audit Logging**: Log security-relevant events

## Deployment

### Development

1. **Hot Reload**: Frontend plugins support hot reload during development
2. **Backend Changes**: Require application restart for backend plugin changes
3. **Database Migrations**: Include migration scripts for plugin-specific entities

### Production

1. **Plugin Validation**: Validate plugins before deployment
2. **Gradual Rollout**: Deploy plugins to a subset of users first
3. **Monitoring**: Monitor plugin performance and error rates
4. **Rollback Plan**: Have a plan to disable or rollback problematic plugins

### Plugin Distribution

1. **Package Format**: Create NuGet packages for backend plugins
2. **Frontend Assets**: Include frontend components in the package
3. **Documentation**: Provide comprehensive documentation
4. **Version Compatibility**: Clearly specify supported application versions

## Troubleshooting

### Common Issues

#### Plugin Not Loading

**Symptoms**: Plugin doesn't appear in the plugin list

**Solutions**:
- Verify the plugin implements `IPlugin` interface
- Check that the plugin assembly is in the plugins directory
- Ensure the plugin has the correct MEF attributes
- Review application logs for loading errors

#### Frontend Component Not Rendering

**Symptoms**: Plugin hook doesn't show the component

**Solutions**:
- Verify the component path is correct
- Check that the hook name matches an existing hook point
- Ensure the frontend component exports a default React component
- Review browser console for JavaScript errors

#### Database Errors

**Symptoms**: Plugin throws database-related exceptions

**Solutions**:
- Verify Entity Framework configuration is correct
- Check that required database tables exist
- Ensure proper connection string configuration
- Review database logs for constraint violations

#### Permission Denied

**Symptoms**: Plugin operations fail with authorization errors

**Solutions**:
- Verify user has required permissions
- Check plugin permission configuration
- Ensure proper role assignment
- Review ACL system configuration

### Debugging Tools

1. **Application Logs**: Check Serilog output for detailed error information
2. **Browser DevTools**: Use React DevTools and browser console for frontend debugging
3. **Database Profiler**: Monitor database queries for performance issues
4. **Plugin Manager**: Use built-in plugin management tools
5. **Health Checks**: Monitor plugin health through health check endpoints

### Performance Optimization

1. **Database Queries**: Optimize LINQ queries and use appropriate indexes
2. **Caching**: Implement caching for frequently accessed data
3. **Lazy Loading**: Use lazy loading for related entities
4. **Bundle Size**: Minimize frontend bundle size with code splitting
5. **Memory Management**: Properly dispose of resources in plugin lifecycle

## Conclusion

The PolyBucket plugin system provides a powerful and flexible way to extend the platform's functionality. By following this guide and the established patterns, developers can create robust plugins that integrate seamlessly with the core application while maintaining security, performance, and user experience standards.

For additional support and examples, refer to:

- [API Documentation](./api-documentation.md)
- [Plugin Examples Repository](./plugin-examples/)
- [Community Forums](https://community.polybucket.com)
- [Plugin Development Discord](https://discord.gg/polybucket-dev)

Happy plugin development! 