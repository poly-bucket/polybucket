using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Http;

public class UpdatePluginStatusRequest
{
    [Required]
    public bool Enabled { get; set; }
}

public class PluginStatusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
