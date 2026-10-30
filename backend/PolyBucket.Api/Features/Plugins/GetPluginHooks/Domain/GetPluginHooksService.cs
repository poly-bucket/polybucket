using PolyBucket.Api.Features.Plugins.GetPluginHooks.Repository;

namespace PolyBucket.Api.Features.Plugins.GetPluginHooks.Domain;

public class GetPluginHooksService(IGetPluginHooksRepository repository) : IGetPluginHooksService
{
    public GetPluginHooksResponse GetHooks()
    {
        var loadedPlugins = repository.GetLoadedPlugins();
        var hooksByExtensionPoint = new Dictionary<string, List<PluginHookInfo>>();

        foreach (var plugin in loadedPlugins)
        {
            if (plugin.FrontendComponents == null)
            {
                continue;
            }

            foreach (var component in plugin.FrontendComponents)
            {
                foreach (var hook in component.Hooks)
                {
                    var extensionPoint = hook.ComponentId;
                    if (!hooksByExtensionPoint.ContainsKey(extensionPoint))
                    {
                        hooksByExtensionPoint[extensionPoint] = new List<PluginHookInfo>();
                    }

                    hooksByExtensionPoint[extensionPoint].Add(new PluginHookInfo
                    {
                        PluginId = plugin.Id,
                        PluginName = plugin.Name,
                        HookName = hook.HookName,
                        Priority = hook.Priority,
                        Configuration = hook.Config
                    });
                }
            }
        }

        foreach (var extensionPoint in hooksByExtensionPoint.Keys.ToList())
        {
            hooksByExtensionPoint[extensionPoint] = hooksByExtensionPoint[extensionPoint]
                .OrderBy(h => h.Priority)
                .ToList();
        }

        return new GetPluginHooksResponse
        {
            ExtensionPoints = hooksByExtensionPoint,
            TotalExtensionPoints = hooksByExtensionPoint.Count,
            TotalHooks = hooksByExtensionPoint.Values.SelectMany(h => h).Count()
        };
    }
}
