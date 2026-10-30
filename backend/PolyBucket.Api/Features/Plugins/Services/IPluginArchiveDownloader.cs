namespace PolyBucket.Api.Features.Plugins.Services;

public interface IPluginArchiveDownloader
{
    Task<PluginArchiveDownloadResult> DownloadRepositoryArchiveAsync(
        string repositoryUrl,
        string branch,
        CancellationToken cancellationToken = default);
}

public class PluginArchiveDownloadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public byte[]? ArchiveBytes { get; set; }
}
