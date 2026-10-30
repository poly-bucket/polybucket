using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.GetPluginSettings.Http;

public class PluginSettingsResponse
{
    public string PluginId { get; set; } = string.Empty;
    public string PluginName { get; set; } = string.Empty;
    public Dictionary<string, PluginSetting> Settings { get; set; } = new();
    public Dictionary<string, object> CurrentValues { get; set; } = new();
    public Dictionary<string, PluginSettingSchema> Schema { get; set; } = new();
}

public class PluginSettingSchema
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
    public object DefaultValue { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}
