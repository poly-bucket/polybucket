# PolyBucket Plugin Development Guide

## Table of Contents

- [Overview](#overview)
- [Plugin Architecture](#plugin-architecture)
- [Backend Plugin Development](#backend-plugin-development)
- [Frontend Plugin Development](#frontend-plugin-development)
- [Comment Plugin Example](#comment-plugin-example)
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