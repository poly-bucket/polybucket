namespace PolyBucket.Api.Features.Plugins.GetPluginHooks.Domain;

public sealed class GetPluginHooksResponse
{
    public Dictionary<string, List<PluginHookInfo>> ExtensionPoints { get; init; } = new();
    public int TotalExtensionPoints { get; init; }
    public int TotalHooks { get; init; }
}

public sealed class PluginHookInfo
{
    public string PluginId { get; init; } = string.Empty;
    public string PluginName { get; init; } = string.Empty;
    public string HookName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}
