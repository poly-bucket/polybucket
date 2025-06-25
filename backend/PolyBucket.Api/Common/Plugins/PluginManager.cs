using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;

namespace PolyBucket.Api.Common.Plugins
{
    public class PluginManager
    {
        private CompositionContainer _container;
        private readonly string _pluginsPath;
        private readonly List<IPlugin> _loadedPlugins = new();

        [ImportMany]
        private IEnumerable<IPlugin> _plugins = new List<IPlugin>();

        public PluginManager(string pluginsPath)
        {
            _pluginsPath = pluginsPath;
            Directory.CreateDirectory(_pluginsPath); // Ensure plugins directory exists
        }

        public async Task InitializeAsync()
        {
            var catalog = new AggregateCatalog();
            
            catalog.Catalogs.Add(new DirectoryCatalog(_pluginsPath));
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            _container = new CompositionContainer(catalog);
            
            try
            {
                _container.ComposeParts(this);
                
                foreach (var plugin in _plugins)
                {
                    try
                    {
                        await plugin.InitializeAsync();
                        _loadedPlugins.Add(plugin);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load plugin: {ex.Message}");
                    }
                }
            }
            catch (CompositionException ex)
            {
                Console.WriteLine($"Failed to compose plugins: {ex.Message}");
            }
        }

        public async Task UnloadAllPluginsAsync()
        {
            foreach (var plugin in _loadedPlugins)
            {
                try
                {
                    await plugin.UnloadAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to unload plugin {plugin.Name}: {ex.Message}");
                }
            }

            _loadedPlugins.Clear();
            _container?.Dispose();
        }

        public IReadOnlyList<IPlugin> GetLoadedPlugins() => _loadedPlugins.AsReadOnly();

        public T? GetPlugin<T>() where T : class, IPlugin
        {
            return _plugins.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetPlugins<T>() where T : class, IPlugin
        {
            return _plugins.OfType<T>();
        }
    }
} 