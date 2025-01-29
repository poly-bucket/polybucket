using Core.Extensions.Models;

namespace Core.Models.Files
{
    public class File : Auditable
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public FileExtension Extension { get; set; }
        public string MimeType { get; set; }
        public long Size { get; set; }
    }
}