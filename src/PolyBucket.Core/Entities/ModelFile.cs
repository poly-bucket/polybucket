using System;

namespace PolyBucket.Core.Entities
{
    public class ModelFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ModelId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileFormat { get; set; }
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; }
        public ModelFileType FileType { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public Model Model { get; set; }
    }
    
    public enum ModelFileType
    {
        MainModel = 0,
        SourceFile = 1,
        RenderImage = 2,
        Documentation = 3,
        PrintSetting = 4,
        AdditionalFile = 5
    }
} 