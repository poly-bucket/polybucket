using System.IO;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType);
        Task<Stream> GetFileAsync(string fileName);
        Task DeleteFileAsync(string fileName);
    }
} 