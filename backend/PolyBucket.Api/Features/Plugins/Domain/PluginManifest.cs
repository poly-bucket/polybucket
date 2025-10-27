using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.Plugins.Domain
{
    public class PluginManifest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("compatibility")]
        public PluginCompatibility Compatibility { get; set; } = new();

        [JsonPropertyName("dependencies")]
        public PluginDependencies Dependencies { get; set; } = new();

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new();

        [JsonPropertyName("hooks")]
        public List<PluginHook> Hooks { get; set; } = new();

        [JsonPropertyName("settings")]
        public List<PluginSetting> Settings { get; set; } = new();

        [JsonPropertyName("assets")]
        public PluginAssets Assets { get; set; } = new();

        [JsonPropertyName("repository")]
        public PluginRepository Repository { get; set; } = new();

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();

        [JsonPropertyName("license")]
        public string License { get; set; } = string.Empty;
    }

    public class PluginCompatibility
    {
        [JsonPropertyName("minVersion")]
        public string MinVersion { get; set; } = "1.0.0";

        [JsonPropertyName("maxVersion")]
        public string MaxVersion { get; set; } = "2.0.0";
    }

    public class PluginDependencies
    {
        [JsonPropertyName("backend")]
        public List<string> Backend { get; set; } = new();

        [JsonPropertyName("frontend")]
        public List<string> Frontend { get; set; } = new();
    }

    public class PluginHook
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("component")]
        public string Component { get; set; } = string.Empty;

        [JsonPropertyName("priority")]
        public int Priority { get; set; } = 100;
    }

    public class PluginSetting
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("default")]
        public object? Default { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class PluginAssets
    {
        [JsonPropertyName("css")]
        public List<string> Css { get; set; } = new();

        [JsonPropertyName("js")]
        public List<string> Js { get; set; } = new();

        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new();
    }

    public class PluginRepository
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
