namespace PolyBucket.Marketplace.Api.Services
{
    public interface IFileService
    {
        Task<string> SavePluginFileAsync(IFormFile file, string pluginId);
        Task<bool> DeletePluginFileAsync(string filePath);
        Task<byte[]?> GetPluginFileAsync(string filePath);
        Task<string> GetPluginFileUrlAsync(string filePath);
    }
}
