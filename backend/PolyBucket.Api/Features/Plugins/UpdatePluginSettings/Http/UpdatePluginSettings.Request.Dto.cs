using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Http;

public class UpdatePluginSettingsRequest
{
    [Required]
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class UpdatePluginSettingsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> UpdatedSettings { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}
