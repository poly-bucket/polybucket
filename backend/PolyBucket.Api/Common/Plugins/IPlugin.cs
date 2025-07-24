using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PolyBucket.Api.Common.Plugins
{
    [InheritedExport]
    public interface IPlugin
    {
        string Id { get; }
        string Name { get; }
        string Version { get; }
        string Author { get; }
        string Description { get; }
        
        /// <summary>
        /// Frontend components provided by this plugin
        /// </summary>
        IEnumerable<PluginComponent> FrontendComponents { get; }
        
        /// <summary>
        /// Plugin settings and configuration schema
        /// </summary>
        PluginMetadata Metadata { get; }
        
        Task InitializeAsync();
        Task UnloadAsync();
    }

    public class PluginComponent
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ComponentPath { get; set; } = string.Empty;
        public ComponentType Type { get; set; }
        public Dictionary<string, object> Props { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
        public List<PluginHook> Hooks { get; set; } = new();
    }

    public class PluginHook
    {
        public string HookName { get; set; } = string.Empty;
        public string ComponentId { get; set; } = string.Empty;
        public int Priority { get; set; } = 100; // Lower number = higher priority
        public Dictionary<string, object> Config { get; set; } = new();
    }

    public enum ComponentType
    {
        Page,
        Widget,
        Modal,
        Sidebar,
        Toolbar,
        Overlay,
        Embed
    }

    public class PluginMetadata
    {
        public string MinimumAppVersion { get; set; } = "1.0.0";
        public string MaximumAppVersion { get; set; } = string.Empty;
        public List<string> RequiredPermissions { get; set; } = new();
        public List<string> OptionalPermissions { get; set; } = new();
        public Dictionary<string, PluginSetting> Settings { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
        public PluginLifecycle Lifecycle { get; set; } = new();
    }

    public class PluginSetting
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PluginSettingType Type { get; set; }
        public object DefaultValue { get; set; } = string.Empty;
        public bool Required { get; set; }
        public List<string> Options { get; set; } = new(); // For select/enum types
    }

    public enum PluginSettingType
    {
        String,
        Number,
        Boolean,
        Select,
        MultiSelect,
        Json
    }

    public class PluginLifecycle
    {
        public bool AutoStart { get; set; } = true;
        public bool CanDisable { get; set; } = true;
        public bool CanUninstall { get; set; } = true;
        public List<string> StartupTasks { get; set; } = new();
        public List<string> ShutdownTasks { get; set; } = new();
    }
} 